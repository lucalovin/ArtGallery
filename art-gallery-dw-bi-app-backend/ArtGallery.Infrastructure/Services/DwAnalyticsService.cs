using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using ArtGallery.Application.DTOs.Reports;
using ArtGallery.Application.Interfaces;
using ArtGallery.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;

namespace ArtGallery.Infrastructure.Services;

/// <summary>
/// Service for Data Warehouse analytics queries with caching support.
/// </summary>
public class DwAnalyticsService : IDwAnalyticsService
{
    private readonly DwDbContext _dwContext;
    private readonly ILogger<DwAnalyticsService> _logger;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromMinutes(5);

    public DwAnalyticsService(
        DwDbContext dwContext,
        ILogger<DwAnalyticsService> logger,
        IMemoryCache cache)
    {
        _dwContext = dwContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<ExhibitionSummaryDto>> GetExhibitionSummaryAsync(
        int? year = null,
        int? artistId = null,
        int limit = 100)
    {
        var cacheKey = $"exhibition_summary_{year}_{artistId}_{limit}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ExhibitionSummaryDto>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogInformation("Fetching exhibition summary. Year: {Year}, ArtistId: {ArtistId}", year, artistId);

        var sql = @"
            SELECT 
                e.EXHIBITION_KEY as ExhibitionKey,
                e.NAME as ExhibitionName,
                e.START_DATE as StartDate,
                e.END_DATE as EndDate,
                e.DURATION_DAYS as DurationDays,
                e.STATUS as Status,
                COALESCE(SUM(f.VISITOR_COUNT), 0) as TotalVisitors,
                COALESCE(SUM(f.TOTAL_REVENUE), 0) as TotalRevenue,
                COALESCE(SUM(f.TICKET_REVENUE), 0) as TicketRevenue,
                COALESCE(SUM(f.MERCHANDISE_REVENUE), 0) as MerchandiseRevenue,
                COUNT(DISTINCT f.ARTWORK_KEY) as ArtworkCount,
                COALESCE(AVG(f.VISIT_DURATION_MINUTES), 0) as AverageVisitDuration,
                CASE WHEN SUM(f.VISITOR_COUNT) > 0 
                     THEN SUM(f.TOTAL_REVENUE) / SUM(f.VISITOR_COUNT) 
                     ELSE 0 END as RevenuePerVisitor
            FROM ART_GALLERY_DW.DIM_EXHIBITION e
            LEFT JOIN ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f 
                ON e.EXHIBITION_KEY = f.EXHIBITION_KEY
            LEFT JOIN ART_GALLERY_DW.DIM_DATE d 
                ON f.DATE_KEY = d.DATE_KEY
            WHERE e.IS_CURRENT = 1
            {0}
            GROUP BY e.EXHIBITION_KEY, e.NAME, e.START_DATE, e.END_DATE, e.DURATION_DAYS, e.STATUS
            ORDER BY e.START_DATE DESC
            FETCH FIRST :limit ROWS ONLY";

        var whereClause = "";
        if (year.HasValue)
            whereClause += " AND d.YEAR = :year";
        if (artistId.HasValue)
            whereClause += " AND f.ARTIST_KEY = :artistId";

        sql = string.Format(sql, whereClause);

        try
        {
            var results = await _dwContext.Database
                .SqlQueryRaw<ExhibitionSummaryDto>(sql,
                    new OracleParameter("limit", limit),
                    new OracleParameter("year", year ?? (object)DBNull.Value),
                    new OracleParameter("artistId", artistId ?? (object)DBNull.Value))
                .ToListAsync();

            _cache.Set(cacheKey, results, _defaultCacheTime);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exhibition summary");
            throw;
        }
    }

    public async Task<IEnumerable<ArtworkInventoryDto>> GetArtworkInventoryAsync(
        int? collectionId = null,
        int? exhibitionId = null,
        int limit = 100)
    {
        _logger.LogInformation("Fetching artwork inventory");

        var sql = @"
            SELECT 
                a.ARTWORK_KEY as ArtworkKey,
                a.TITLE as Title,
                ar.FULL_NAME as ArtistName,
                a.CREATION_YEAR as CreationYear,
                a.MEDIUM as Medium,
                a.COLLECTION_TYPE as CollectionType,
                a.STATUS as Status,
                a.ESTIMATED_VALUE as EstimatedValue,
                i.COVERAGE_AMOUNT as InsuranceValue,
                COUNT(DISTINCT f.EXHIBITION_KEY) as ExhibitionCount,
                MAX(d.FULL_DATE) as LastExhibitionDate
            FROM ART_GALLERY_DW.DIM_ARTWORK a
            LEFT JOIN ART_GALLERY_DW.DIM_ARTIST ar ON a.ARTIST_KEY = ar.ARTIST_KEY AND ar.IS_CURRENT = 1
            LEFT JOIN ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f ON a.ARTWORK_KEY = f.ARTWORK_KEY
            LEFT JOIN ART_GALLERY_DW.DIM_DATE d ON f.DATE_KEY = d.DATE_KEY
            LEFT JOIN ART_GALLERY_DW.DIM_INSURANCE i ON f.INSURANCE_KEY = i.INSURANCE_KEY AND i.IS_CURRENT = 1
            WHERE a.IS_CURRENT = 1
            GROUP BY a.ARTWORK_KEY, a.TITLE, ar.FULL_NAME, a.CREATION_YEAR, 
                     a.MEDIUM, a.COLLECTION_TYPE, a.STATUS, a.ESTIMATED_VALUE, i.COVERAGE_AMOUNT
            ORDER BY a.ESTIMATED_VALUE DESC NULLS LAST
            FETCH FIRST :limit ROWS ONLY";

        var results = await _dwContext.Database
            .SqlQueryRaw<ArtworkInventoryDto>(sql,
                new OracleParameter("limit", limit))
            .ToListAsync();

        return results;
    }

    public async Task<InsuranceAnalysisDto> GetInsuranceAnalysisAsync(
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var cacheKey = $"insurance_analysis_{dateFrom:yyyyMMdd}_{dateTo:yyyyMMdd}";
        
        if (_cache.TryGetValue(cacheKey, out InsuranceAnalysisDto? cached) && cached != null)
        {
            return cached;
        }

        _logger.LogInformation("Fetching insurance analysis");

        dateFrom ??= DateTime.Today.AddYears(-1);
        dateTo ??= DateTime.Today;

        var result = new InsuranceAnalysisDto();

        // Get insurance metrics using LINQ
        var insuranceData = await _dwContext.DimInsurances
            .Where(i => i.IsCurrent)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalCoverage = g.Sum(i => i.CoverageAmount ?? 0),
                TotalPremium = g.Sum(i => i.Premium ?? 0),
                ActivePolicies = g.Count(i => i.Status == "Active"),
                ExpiredPolicies = g.Count(i => i.Status == "Expired"),
                ExpiringThisMonth = g.Count(i => i.EndDate.HasValue && 
                    i.EndDate.Value >= DateTime.Today && 
                    i.EndDate.Value <= DateTime.Today.AddMonths(1))
            })
            .FirstOrDefaultAsync();

        if (insuranceData != null)
        {
            result.TotalCoverageAmount = insuranceData.TotalCoverage;
            result.TotalPremiumAmount = insuranceData.TotalPremium;
            result.ActivePolicies = insuranceData.ActivePolicies;
            result.ExpiredPolicies = insuranceData.ExpiredPolicies;
            result.ExpiringThisMonth = insuranceData.ExpiringThisMonth;
        }

        // By provider
        result.ByProvider = await _dwContext.DimInsurances
            .Where(i => i.IsCurrent && i.Provider != null)
            .GroupBy(i => i.Provider!)
            .Select(g => new InsuranceByProviderDto
            {
                Provider = g.Key,
                PolicyCount = g.Count(),
                TotalCoverage = g.Sum(i => i.CoverageAmount ?? 0),
                TotalPremium = g.Sum(i => i.Premium ?? 0)
            })
            .OrderByDescending(x => x.TotalCoverage)
            .ToListAsync();

        // By coverage type
        result.ByCoverageType = await _dwContext.DimInsurances
            .Where(i => i.IsCurrent && i.CoverageType != null)
            .GroupBy(i => i.CoverageType!)
            .Select(g => new InsuranceByCoverageTypeDto
            {
                CoverageType = g.Key,
                PolicyCount = g.Count(),
                TotalCoverage = g.Sum(i => i.CoverageAmount ?? 0)
            })
            .OrderByDescending(x => x.PolicyCount)
            .ToListAsync();

        _cache.Set(cacheKey, result, _defaultCacheTime);
        return result;
    }

    public async Task<IEnumerable<ArtistPerformanceDto>> GetArtistPerformanceAsync(
        string orderBy = "estimated_value_desc",
        int limit = 50)
    {
        _logger.LogInformation("Fetching artist performance. OrderBy: {OrderBy}", orderBy);

        var query = from ar in _dwContext.DimArtists
                    where ar.IsCurrent
                    join a in _dwContext.DimArtworks.Where(x => x.IsCurrent) 
                        on ar.Id equals a.ArtistKey into artworks
                    from artwork in artworks.DefaultIfEmpty()
                    group new { ar, artwork } by new 
                    { 
                        ar.Id, 
                        ar.FullName, 
                        ar.Nationality, 
                        ar.ArtMovement 
                    } into g
                    select new ArtistPerformanceDto
                    {
                        ArtistKey = g.Key.Id,
                        ArtistName = g.Key.FullName,
                        Nationality = g.Key.Nationality ?? "",
                        ArtMovement = g.Key.ArtMovement ?? "",
                        TotalArtworks = g.Count(x => x.artwork != null),
                        TotalEstimatedValue = g.Sum(x => x.artwork != null ? x.artwork.EstimatedValue ?? 0 : 0),
                        AverageArtworkValue = g.Average(x => x.artwork != null ? x.artwork.EstimatedValue ?? 0 : 0)
                    };

        query = orderBy.ToLower() switch
        {
            "name" => query.OrderBy(x => x.ArtistName),
            "name_desc" => query.OrderByDescending(x => x.ArtistName),
            "artworks" => query.OrderByDescending(x => x.TotalArtworks),
            _ => query.OrderByDescending(x => x.TotalEstimatedValue)
        };

        return await query.Take(limit).ToListAsync();
    }

    public async Task<VisitorTrendsDto> GetVisitorTrendsAsync(
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string groupBy = "month")
    {
        _logger.LogInformation("Fetching visitor trends. GroupBy: {GroupBy}", groupBy);

        dateFrom ??= DateTime.Today.AddYears(-1);
        dateTo ??= DateTime.Today;

        var result = new VisitorTrendsDto();

        // Overall metrics from fact table
        var factQuery = _dwContext.FactExhibitionActivities
            .Join(_dwContext.DimDates,
                f => f.DateKey,
                d => d.DateKey,
                (f, d) => new { f, d })
            .Where(x => x.d.FullDate >= dateFrom && x.d.FullDate <= dateTo);

        var overallData = await factQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalVisitors = g.Sum(x => x.f.VisitorCount),
                AvgDuration = g.Average(x => x.f.VisitDurationMinutes ?? 0),
                TotalRevenue = g.Sum(x => x.f.TotalRevenue)
            })
            .FirstOrDefaultAsync();

        if (overallData != null)
        {
            result.TotalVisitors = overallData.TotalVisitors;
            result.AverageVisitDuration = (decimal)overallData.AvgDuration;
            result.AverageSpendPerVisit = overallData.TotalVisitors > 0 
                ? overallData.TotalRevenue / overallData.TotalVisitors 
                : 0;
        }

        // Monthly trend data
        result.TrendData = await factQuery
            .GroupBy(x => new { x.d.Year, x.d.MonthNumber })
            .Select(g => new VisitorTrendDataPoint
            {
                Period = g.Key.Year + "-" + g.Key.MonthNumber.ToString("D2"),
                VisitorCount = g.Sum(x => x.f.VisitorCount),
                Revenue = g.Sum(x => x.f.TotalRevenue),
                AverageSpend = g.Sum(x => x.f.VisitorCount) > 0 
                    ? g.Sum(x => x.f.TotalRevenue) / g.Sum(x => x.f.VisitorCount) 
                    : 0
            })
            .OrderBy(x => x.Period)
            .ToListAsync();

        return result;
    }

    public async Task<RevenueBreakdownDto> GetRevenueBreakdownAsync(
        int? year = null,
        string dimension = "exhibition")
    {
        var cacheKey = $"revenue_breakdown_{year}_{dimension}";
        
        if (_cache.TryGetValue(cacheKey, out RevenueBreakdownDto? cached) && cached != null)
        {
            return cached;
        }

        _logger.LogInformation("Fetching revenue breakdown. Year: {Year}, Dimension: {Dimension}", year, dimension);

        year ??= DateTime.Today.Year;

        var result = new RevenueBreakdownDto();

        // Total revenue from fact table
        var factQuery = _dwContext.FactExhibitionActivities
            .Where(f => f.PartitionYear == year);

        var totals = await factQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Sum(f => f.TotalRevenue),
                Ticket = g.Sum(f => f.TicketRevenue),
                Merchandise = g.Sum(f => f.MerchandiseRevenue)
            })
            .FirstOrDefaultAsync();

        if (totals != null)
        {
            result.TotalRevenue = totals.Total;
            result.TicketRevenue = totals.Ticket;
            result.MerchandiseRevenue = totals.Merchandise;
            result.OtherRevenue = totals.Total - totals.Ticket - totals.Merchandise;
        }

        // Breakdown by exhibition
        if (dimension.ToLower() == "exhibition")
        {
            result.BreakdownByDimension = await (
                from f in factQuery
                join e in _dwContext.DimExhibitions.Where(x => x.IsCurrent)
                    on f.ExhibitionKey equals e.Id
                group f by e.Name into g
                select new RevenueDimensionDto
                {
                    DimensionValue = g.Key,
                    Revenue = g.Sum(x => x.TotalRevenue),
                    Percentage = result.TotalRevenue > 0 
                        ? Math.Round(g.Sum(x => x.TotalRevenue) * 100 / result.TotalRevenue, 2)
                        : 0
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();
        }

        _cache.Set(cacheKey, result, _defaultCacheTime);
        return result;
    }

    public async Task<KpiDashboardDto> GetKpiDashboardAsync()
    {
        var cacheKey = "kpi_dashboard";
        
        if (_cache.TryGetValue(cacheKey, out KpiDashboardDto? cached) && cached != null)
        {
            return cached;
        }

        _logger.LogInformation("Fetching KPI dashboard metrics");

        var result = new KpiDashboardDto { AsOf = DateTime.UtcNow };
        var currentYear = DateTime.Today.Year;

        // Collection metrics
        var artworkStats = await _dwContext.DimArtworks
            .Where(a => a.IsCurrent)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                TotalValue = g.Sum(a => a.EstimatedValue ?? 0),
                OnDisplay = g.Count(a => a.Status == "OnDisplay"),
                InStorage = g.Count(a => a.Status == "InStorage" || a.Status == "Available"),
                OnLoan = g.Count(a => a.Status == "OnLoan"),
                InRestoration = g.Count(a => a.Status == "InRestoration")
            })
            .FirstOrDefaultAsync();

        if (artworkStats != null)
        {
            result.TotalArtworks = artworkStats.Total;
            result.TotalCollectionValue = artworkStats.TotalValue;
            result.ArtworksOnDisplay = artworkStats.OnDisplay;
            result.ArtworksInStorage = artworkStats.InStorage;
            result.ArtworksOnLoan = artworkStats.OnLoan;
            result.ArtworksInRestoration = artworkStats.InRestoration;
        }

        // Artist count
        result.TotalArtists = await _dwContext.DimArtists.CountAsync(a => a.IsCurrent);

        // Exhibition metrics
        result.TotalExhibitions = await _dwContext.DimExhibitions.CountAsync(e => e.IsCurrent);
        result.ActiveExhibitions = await _dwContext.DimExhibitions
            .CountAsync(e => e.IsCurrent && e.Status == "Active");
        result.UpcomingExhibitions = await _dwContext.DimExhibitions
            .CountAsync(e => e.IsCurrent && e.Status == "Planning" && e.StartDate > DateTime.Today);

        // YTD metrics from fact table
        var ytdData = await _dwContext.FactExhibitionActivities
            .Where(f => f.PartitionYear == currentYear)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Visitors = g.Sum(f => f.VisitorCount),
                Revenue = g.Sum(f => f.TotalRevenue)
            })
            .FirstOrDefaultAsync();

        if (ytdData != null)
        {
            result.TotalVisitorsYtd = ytdData.Visitors;
            result.TotalRevenueYtd = ytdData.Revenue;
        }

        // Insurance metrics
        result.TotalInsuranceValue = await _dwContext.DimInsurances
            .Where(i => i.IsCurrent && i.Status == "Active")
            .SumAsync(i => i.CoverageAmount ?? 0);

        result.ExpiringInsurancePolicies = await _dwContext.DimInsurances
            .CountAsync(i => i.IsCurrent && 
                           i.EndDate.HasValue && 
                           i.EndDate.Value <= DateTime.Today.AddMonths(1));

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<IEnumerable<PartitionStatDto>> GetPartitionStatsAsync()
    {
        _logger.LogInformation("Fetching partition statistics");

        // Return empty list if partitioning is not set up - this query is Oracle-specific
        try
        {
            var sql = @"
                SELECT 
                    TABLE_NAME as TableName,
                    PARTITION_NAME as PartitionName,
                    TO_NUMBER(SUBSTR(PARTITION_NAME, -4)) as PartitionYear,
                    NUM_ROWS as RowCount,
                    ROUND(BYTES / 1024 / 1024, 2) as SizeMb,
                    LAST_ANALYZED as LastAnalyzed
                FROM USER_TAB_PARTITIONS utp
                JOIN USER_SEGMENTS us ON utp.TABLE_NAME = us.SEGMENT_NAME 
                                      AND utp.PARTITION_NAME = us.PARTITION_NAME
                WHERE utp.TABLE_NAME = 'FACT_EXHIBITION_ACTIVITY'
                ORDER BY PARTITION_NAME";

            var results = await _dwContext.Database
                .SqlQueryRaw<PartitionStatDto>(sql)
                .ToListAsync();
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch partition stats - table may not be partitioned");
            return new List<PartitionStatDto>();
        }
    }
}
