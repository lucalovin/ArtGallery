﻿using Microsoft.EntityFrameworkCore;
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
    // Reduced cache time to allow near-real-time refresh of analytics data
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromSeconds(5);

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

        // Note: DW schema has START_DATE_KEY/END_DATE_KEY (not START_DATE/END_DATE)
        // and TITLE (not NAME). No VISITOR_COUNT, REVENUE, DURATION_DAYS, STATUS, IS_CURRENT columns.
        var sql = @"
            SELECT 
                e.EXHIBITION_KEY as ExhibitionKey,
                e.TITLE as ExhibitionName,
                sd.CALENDAR_DATE as StartDate,
                ed.CALENDAR_DATE as EndDate,
                0 as DurationDays,
                'N/A' as Status,
                0 as TotalVisitors,
                0 as TotalRevenue,
                0 as TicketRevenue,
                0 as MerchandiseRevenue,
                COUNT(DISTINCT f.ARTWORK_KEY) as ArtworkCount,
                0 as AverageVisitDuration,
                0 as RevenuePerVisitor
            FROM ART_GALLERY_DW.DIM_EXHIBITION e
            LEFT JOIN ART_GALLERY_DW.DIM_DATE sd ON e.START_DATE_KEY = sd.DATE_KEY
            LEFT JOIN ART_GALLERY_DW.DIM_DATE ed ON e.END_DATE_KEY = ed.DATE_KEY
            LEFT JOIN ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f 
                ON e.EXHIBITION_KEY = f.EXHIBITION_KEY
            LEFT JOIN ART_GALLERY_DW.DIM_DATE d 
                ON f.DATE_KEY = d.DATE_KEY
            WHERE 1=1
            {0}
            GROUP BY e.EXHIBITION_KEY, e.TITLE, sd.CALENDAR_DATE, ed.CALENDAR_DATE
            ORDER BY sd.CALENDAR_DATE DESC NULLS LAST
            FETCH FIRST :limit ROWS ONLY";

        var whereClause = "";
        if (year.HasValue)
            whereClause += " AND d.CALENDAR_YEAR = :year";
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

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_defaultCacheTime)
                .SetSize(1); // Set size for cache entry
            
            _cache.Set(cacheKey, results, cacheEntryOptions);
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

        // Note: DW schema has simplified structure - many columns don't exist
        // Using available columns from DIM_ARTWORK, DIM_ARTIST, FACT_EXHIBITION_ACTIVITY
        var sql = @"
            SELECT 
                a.ARTWORK_KEY as ArtworkKey,
                a.TITLE as Title,
                ar.NAME as ArtistName,
                a.YEAR_CREATED as CreationYear,
                a.MEDIUM as Medium,
                'N/A' as CollectionType,
                'N/A' as Status,
                a.ESTIMATED_VALUE as EstimatedValue,
                NVL(MAX(f.INSURED_AMOUNT), 0) as InsuranceValue,
                COUNT(DISTINCT f.EXHIBITION_KEY) as ExhibitionCount,
                MAX(d.CALENDAR_DATE) as LastExhibitionDate
            FROM ART_GALLERY_DW.DIM_ARTWORK a
            LEFT JOIN ART_GALLERY_DW.DIM_ARTIST ar ON a.ARTIST_KEY = ar.ARTIST_KEY
            LEFT JOIN ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f ON a.ARTWORK_KEY = f.ARTWORK_KEY
            LEFT JOIN ART_GALLERY_DW.DIM_DATE d ON f.DATE_KEY = d.DATE_KEY
            WHERE 1=1
            {0}
            GROUP BY a.ARTWORK_KEY, a.TITLE, ar.NAME, a.YEAR_CREATED, 
                     a.MEDIUM, a.ESTIMATED_VALUE
            ORDER BY a.ESTIMATED_VALUE DESC NULLS LAST
            FETCH FIRST :limit ROWS ONLY";

        var whereClause = "";
        if (collectionId.HasValue)
            whereClause += " AND a.COLLECTION_KEY = :collectionId";
        if (exhibitionId.HasValue)
            whereClause += " AND f.EXHIBITION_KEY = :exhibitionId";

        sql = string.Format(sql, whereClause);

        var parameters = new List<OracleParameter>
        {
            new OracleParameter("limit", limit)
        };
        
        if (collectionId.HasValue)
            parameters.Add(new OracleParameter("collectionId", collectionId.Value));
        if (exhibitionId.HasValue)
            parameters.Add(new OracleParameter("exhibitionId", exhibitionId.Value));

        var results = await _dwContext.Database
            .SqlQueryRaw<ArtworkInventoryDto>(sql, parameters.ToArray())
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

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_defaultCacheTime)
            .SetSize(1);
        
        _cache.Set(cacheKey, result, cacheEntryOptions);
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

        // Note: The current DW schema doesn't have visitor count or revenue data in FACT_EXHIBITION_ACTIVITY
        // This method returns metrics based on available data: review counts and ratings
        
        var factQuery = _dwContext.FactExhibitionActivities
            .Join(_dwContext.DimDates,
                f => f.DateKey,
                d => d.DateKey,
                (f, d) => new { f, d })
            .Where(x => x.d.CalendarDate >= dateFrom && x.d.CalendarDate <= dateTo);

        var overallData = await factQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalReviews = g.Sum(x => x.f.ReviewCount ?? 0),
                AvgRating = g.Average(x => x.f.AvgRating ?? 0),
                TotalArtworks = g.Count()
            })
            .FirstOrDefaultAsync();

        if (overallData != null)
        {
            // Map review count to visitor count as a proxy
            result.TotalVisitors = overallData.TotalReviews;
            result.AverageVisitDuration = (decimal)overallData.AvgRating; // Using rating as a proxy
            result.AverageSpendPerVisit = 0; // No revenue data available
        }

        // Monthly trend data based on review counts
        // First get the data from DB, then format the period string in C#
        var monthlyData = await factQuery
            .GroupBy(x => new { x.d.CalendarYear, x.d.CalendarMonth })
            .Select(g => new
            {
                Year = g.Key.CalendarYear,
                Month = g.Key.CalendarMonth,
                ReviewCount = g.Sum(x => x.f.ReviewCount ?? 0)
            })
            .ToListAsync();

        // Format the period string in C# code (not in SQL)
        result.TrendData = monthlyData
            .Select(x => new VisitorTrendDataPoint
            {
                Period = $"{x.Year}-{x.Month:D2}", // Format in C# not SQL
                VisitorCount = x.ReviewCount, // Using review count as proxy
                Revenue = 0, // No revenue data available
                AverageSpend = 0 // No revenue data available
            })
            .OrderBy(x => x.Period)
            .ToList();

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

        // Note: The current DW schema doesn't have revenue or partition year data
        // This method returns empty/zero values
        
        result.TotalRevenue = 0;
        result.TicketRevenue = 0;
        result.MerchandiseRevenue = 0;
        result.OtherRevenue = 0;
        result.BreakdownByDimension = new List<RevenueDimensionDto>();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(_defaultCacheTime)
            .SetSize(1);

        _cache.Set(cacheKey, result, cacheEntryOptions);
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
        result.TotalArtists = await _dwContext.DimArtists.CountAsync();

        // Exhibition metrics
        result.TotalExhibitions = await _dwContext.DimExhibitions.CountAsync();
        result.ActiveExhibitions = 0; // No status field in schema
        result.UpcomingExhibitions = 0; // No status field in schema

        // YTD metrics from fact table - using review counts as proxy
        var ytdData = await _dwContext.FactExhibitionActivities
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Reviews = g.Sum(f => f.ReviewCount ?? 0),
                AvgRating = g.Average(f => f.AvgRating ?? 0)
            })
            .FirstOrDefaultAsync();

        if (ytdData != null)
        {
            result.TotalVisitorsYtd = ytdData.Reviews; // Using review count as proxy
            result.TotalRevenueYtd = 0; // No revenue data in schema
        }

        // Insurance metrics - simplified since schema is different
        result.TotalInsuranceValue = await _dwContext.FactExhibitionActivities
            .SumAsync(f => f.InsuredAmount ?? 0);

        result.ExpiringInsurancePolicies = 0; // Not available in current schema

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSize(1);
        
        _cache.Set(cacheKey, result, cacheEntryOptions);
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

    // ============================================================================
    // Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
    // ============================================================================

    /// <summary>
    /// Query 1: Get top N artists by artwork count with total estimated value.
    /// </summary>
    public async Task<List<ArtistStatisticsDto>> GetTopArtistsByArtworkCountAsync(int topN = 10)
    {
        var cacheKey = $"top_artists_{topN}";
        
        if (_cache.TryGetValue(cacheKey, out List<ArtistStatisticsDto>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogInformation("Fetching top {TopN} artists by artwork count", topN);

        var sql = @"
            SELECT * FROM (
                SELECT
                    da.NAME as ArtistName,
                    COUNT(DISTINCT daw.ARTWORK_KEY) as ArtworkCount,
                    SUM(NVL(f.ESTIMATED_VALUE, 0)) as TotalValue,
                    AVG(NVL(f.ESTIMATED_VALUE, 0)) as AverageValue
                FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
                JOIN ART_GALLERY_DW.DIM_ARTIST da ON f.ARTIST_KEY = da.ARTIST_KEY
                JOIN ART_GALLERY_DW.DIM_ARTWORK daw ON f.ARTWORK_KEY = daw.ARTWORK_KEY
                GROUP BY da.NAME
                ORDER BY ArtworkCount DESC
            )
            WHERE ROWNUM <= :topN";

        try
        {
            var results = await _dwContext.Database
                .SqlQueryRaw<ArtistStatisticsDto>(sql, new OracleParameter("topN", topN))
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_defaultCacheTime)
                .SetSize(1);
            
            _cache.Set(cacheKey, results, cacheEntryOptions);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top artists by artwork count");
            throw;
        }
    }

    /// <summary>
    /// Query 2: Get collection value breakdown by art medium and collection type.
    /// </summary>
    public async Task<List<CategoryValueDto>> GetValueByMediumAndCollectionAsync()
    {
        var cacheKey = "value_by_medium_collection";
        
        if (_cache.TryGetValue(cacheKey, out List<CategoryValueDto>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogInformation("Fetching value by medium and collection");

        // Query directly from DIM_ARTWORK to get accurate artwork counts and values.
        // Previously queried through FACT_EXHIBITION_ACTIVITY which caused duplicate
        // counts when an artwork appeared in multiple exhibitions.
        var sql = @"
            SELECT
                NVL(daw.MEDIUM, 'Unknown') as MediumType,
                NVL(dc.NAME, 'Unknown') as CollectionName,
                COUNT(daw.ARTWORK_KEY) as ArtworkCount,
                SUM(NVL(daw.ESTIMATED_VALUE, 0)) as TotalValue,
                AVG(NVL(daw.ESTIMATED_VALUE, 0)) as AverageValue
            FROM ART_GALLERY_DW.DIM_ARTWORK daw
            LEFT JOIN ART_GALLERY_DW.DIM_COLLECTION dc ON daw.COLLECTION_KEY = dc.COLLECTION_KEY
            GROUP BY daw.MEDIUM, dc.NAME
            ORDER BY TotalValue DESC";

        try
        {
            var results = await _dwContext.Database
                .SqlQueryRaw<CategoryValueDto>(sql)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_defaultCacheTime)
                .SetSize(1);
            
            _cache.Set(cacheKey, results, cacheEntryOptions);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching value by medium and collection");
            throw;
        }
    }

    /// <summary>
    /// Query 3: Get monthly exhibition activity metrics for the last N months.
    /// </summary>
    public async Task<List<MonthlyActivityDto>> GetMonthlyExhibitionActivityAsync(int months = 12)
    {
        var cacheKey = $"monthly_activity_{months}";
        
        if (_cache.TryGetValue(cacheKey, out List<MonthlyActivityDto>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogInformation("Fetching monthly exhibition activity for last {Months} months", months);

        var sql = @"
            SELECT
                dd.MONTH_NAME as MonthName,
                dd.CALENDAR_YEAR as Year,
                COUNT(DISTINCT f.EXHIBITION_KEY) as ExhibitionCount,
                COUNT(DISTINCT f.ARTWORK_KEY) as ArtworksExhibited,
                SUM(NVL(f.ESTIMATED_VALUE, 0)) as TotalArtworkValue,
                AVG(f.AVG_RATING) as AverageRating
            FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
            JOIN ART_GALLERY_DW.DIM_DATE dd ON f.DATE_KEY = dd.DATE_KEY
            WHERE dd.CALENDAR_DATE >= ADD_MONTHS(SYSDATE, :negMonths)
            GROUP BY dd.CALENDAR_YEAR, dd.MONTH_NAME, dd.CALENDAR_MONTH
            ORDER BY dd.CALENDAR_YEAR, dd.CALENDAR_MONTH";

        try
        {
            var results = await _dwContext.Database
                .SqlQueryRaw<MonthlyActivityDto>(sql, new OracleParameter("negMonths", -months))
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_defaultCacheTime)
                .SetSize(1);
            
            _cache.Set(cacheKey, results, cacheEntryOptions);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly exhibition activity");
            throw;
        }
    }

    /// <summary>
    /// Query 4: Get location/gallery distribution of artworks.
    /// </summary>
    public async Task<List<LocationDistributionDto>> GetLocationDistributionAsync()
    {
        var cacheKey = "location_distribution";
        
        if (_cache.TryGetValue(cacheKey, out List<LocationDistributionDto>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogInformation("Fetching location distribution");

        var sql = @"
            SELECT
                NVL(dl.NAME, 'Unknown') as LocationName,
                NVL(dl.GALLERY_ROOM, 'N/A') as GalleryRoom,
                NVL(dl.TYPE, 'Unknown') as LocationType,
                COUNT(DISTINCT f.ARTWORK_KEY) as ArtworksCount,
                SUM(NVL(f.ESTIMATED_VALUE, 0)) as TotalValue,
                ROUND(COUNT(DISTINCT f.ARTWORK_KEY) * 100.0 / 
                    NULLIF(SUM(COUNT(DISTINCT f.ARTWORK_KEY)) OVER(), 0), 2) as Percentage
            FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
            JOIN ART_GALLERY_DW.DIM_LOCATION dl ON f.LOCATION_KEY = dl.LOCATION_KEY
            GROUP BY dl.NAME, dl.GALLERY_ROOM, dl.TYPE
            ORDER BY ArtworksCount DESC";

        try
        {
            var results = await _dwContext.Database
                .SqlQueryRaw<LocationDistributionDto>(sql)
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_defaultCacheTime)
                .SetSize(1);
            
            _cache.Set(cacheKey, results, cacheEntryOptions);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching location distribution");
            throw;
        }
    }

    /// <summary>
    /// Query 5: Get annual exhibition value trends with year-over-year growth.
    /// </summary>
    public async Task<List<AnnualTrendDto>> GetAnnualExhibitionTrendsAsync(int years = 5)
    {
        var cacheKey = $"annual_trends_{years}";
        
        if (_cache.TryGetValue(cacheKey, out List<AnnualTrendDto>? cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogInformation("Fetching annual exhibition trends for last {Years} years", years);

        var sql = @"
            SELECT
                dd.CALENDAR_YEAR as Year,
                COUNT(DISTINCT f.EXHIBITION_KEY) as ExhibitionsCount,
                COUNT(DISTINCT f.ARTWORK_KEY) as ArtworksCount,
                SUM(NVL(f.ESTIMATED_VALUE, 0)) as TotalArtworkValue,
                AVG(NVL(f.ESTIMATED_VALUE, 0)) as AverageArtworkValue,
                LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) as PreviousYearValue,
                CASE 
                    WHEN LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) IS NOT NULL 
                         AND LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR) > 0
                    THEN ROUND(
                        ((SUM(NVL(f.ESTIMATED_VALUE, 0)) - 
                          LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR)) * 100.0 / 
                          LAG(SUM(NVL(f.ESTIMATED_VALUE, 0))) OVER (ORDER BY dd.CALENDAR_YEAR)), 2)
                    ELSE NULL
                END as YoyGrowthRate
            FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
            JOIN ART_GALLERY_DW.DIM_DATE dd ON f.DATE_KEY = dd.DATE_KEY
            WHERE dd.CALENDAR_YEAR >= EXTRACT(YEAR FROM SYSDATE) - :years
            GROUP BY dd.CALENDAR_YEAR
            ORDER BY dd.CALENDAR_YEAR";

        try
        {
            var results = await _dwContext.Database
                .SqlQueryRaw<AnnualTrendDto>(sql, new OracleParameter("years", years))
                .ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_defaultCacheTime)
                .SetSize(1);
            
            _cache.Set(cacheKey, results, cacheEntryOptions);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching annual exhibition trends");
            throw;
        }
    }
}
