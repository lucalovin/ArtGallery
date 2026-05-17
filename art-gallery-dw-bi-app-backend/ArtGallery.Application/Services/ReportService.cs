using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Reports;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class ReportService : IReportService
{
    private readonly IRepository<Artwork> _artworkRepository;
    private readonly IRepository<Exhibition> _exhibitionRepository;
    private readonly IRepository<Visitor> _visitorRepository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<Loan> _loanRepository;
    private readonly IRepository<Insurance> _insuranceRepository;
    private readonly IRepository<Restoration> _restorationRepository;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public ReportService(
        IRepository<Artwork> artworkRepository,
        IRepository<Exhibition> exhibitionRepository,
        IRepository<Visitor> visitorRepository,
        IRepository<Staff> staffRepository,
        IRepository<Loan> loanRepository,
        IRepository<Insurance> insuranceRepository,
        IRepository<Restoration> restorationRepository,
        IDataSourceContext ds,
        IConfiguration configuration)
    {
        _artworkRepository = artworkRepository;
        _exhibitionRepository = exhibitionRepository;
        _visitorRepository = visitorRepository;
        _staffRepository = staffRepository;
        _loanRepository = loanRepository;
        _insuranceRepository = insuranceRepository;
        _restorationRepository = restorationRepository;
        _ds = ds;
        _configuration = configuration;
    }

    private string? GetGlobalConnectionString()
    {
        return _configuration.GetConnectionString("BddGlobalConnection");
    }

    private async Task<int> ExecuteScalarIntAsync(OracleConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
            return 0;

        return Convert.ToInt32(result);
    }

    private async Task<decimal> ExecuteScalarDecimalAsync(OracleConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
            return 0m;

        return Convert.ToDecimal(result);
    }

    public async Task<KpiDashboardDto> GetKpisAsync()
    {
        return await GetGlobalKpisAsync();
    }

    private async Task<KpiDashboardDto> GetGlobalKpisAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new KpiDashboardDto
            {
                GeneratedAt = DateTime.UtcNow
            };
        }

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var totalArtworks = await ExecuteScalarIntAsync(connection, @"
            SELECT COUNT(*)
            FROM ARTWORK_CORE@link_eu c
            JOIN ARTWORK_DETAILS@link_am d
              ON d.ARTWORK_ID = c.ARTWORK_ID");

        var totalCollectionValue = await ExecuteScalarDecimalAsync(connection, @"
            SELECT NVL(SUM(d.ESTIMATED_VALUE), 0)
            FROM ARTWORK_CORE@link_eu c
            JOIN ARTWORK_DETAILS@link_am d
              ON d.ARTWORK_ID = c.ARTWORK_ID");

        var totalVisitors = await ExecuteScalarIntAsync(connection, @"
            SELECT COUNT(*)
            FROM VISITOR");

        var totalStaff = await ExecuteScalarIntAsync(connection, @"
            SELECT COUNT(*)
            FROM STAFF");

        var activeExhibitions = await ExecuteScalarIntAsync(connection, @"
            SELECT COUNT(*)
            FROM (
                SELECT START_DATE, END_DATE FROM EXHIBITION_AM@link_am
                UNION ALL
                SELECT START_DATE, END_DATE FROM EXHIBITION_EU@link_eu
            )
            WHERE START_DATE <= TRUNC(SYSDATE)
              AND END_DATE >= TRUNC(SYSDATE)");

        var activeLoans = await ExecuteScalarIntAsync(connection, @"
            SELECT COUNT(*)
            FROM (
                SELECT END_DATE FROM LOAN_AM@link_am
                UNION ALL
                SELECT END_DATE FROM LOAN_EU@link_eu
            )
            WHERE END_DATE IS NULL
               OR END_DATE >= TRUNC(SYSDATE)");

        var restorations = await ExecuteScalarIntAsync(connection, @"
            SELECT COUNT(*)
            FROM RESTORATION
            WHERE END_DATE IS NULL");

        var insuranceCoverage = await ExecuteScalarDecimalAsync(connection, @"
            SELECT NVL(SUM(i.INSURED_AMOUNT), 0)
            FROM INSURANCE i
            LEFT JOIN INSURANCE_POLICY p
              ON p.POLICY_ID = i.POLICY_ID
            WHERE p.POLICY_ID IS NULL
               OR p.END_DATE >= TRUNC(SYSDATE)");

        return new KpiDashboardDto
        {
            TotalArtworks = totalArtworks,
            TotalVisitors = totalVisitors,
            ActiveExhibitions = activeExhibitions,
            TotalStaff = totalStaff,
            TotalCollectionValue = totalCollectionValue,
            ActiveLoans = activeLoans,
            ArtworksUnderRestoration = restorations,
            TotalInsuranceCoverage = insuranceCoverage,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<VisitorTrendDto>> GetVisitorTrendsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return await GetGlobalVisitorTrendsAsync(startDate, endDate);
    }

    private async Task<IEnumerable<VisitorTrendDto>> GetGlobalVisitorTrendsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = endDate ?? DateTime.UtcNow;

        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return BuildEmptyVisitorMonths(start, end);

        var trends = new List<VisitorTrendDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                TRUNC(JOIN_DATE, 'MM') AS MONTH_DATE,
                COUNT(*) AS VISITOR_COUNT,
                SUM(CASE
                    WHEN MEMBERSHIP_TYPE IS NOT NULL
                     AND MEMBERSHIP_TYPE <> 'None'
                     AND MEMBERSHIP_TYPE <> ''
                    THEN 1
                    ELSE 0
                END) AS NEW_MEMBERS
            FROM VISITOR
            WHERE JOIN_DATE IS NOT NULL
              AND JOIN_DATE >= :startDate
              AND JOIN_DATE <= :endDate
            GROUP BY TRUNC(JOIN_DATE, 'MM')
            ORDER BY MONTH_DATE";

        command.Parameters.Add(new OracleParameter("startDate", start));
        command.Parameters.Add(new OracleParameter("endDate", end));

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trends.Add(new VisitorTrendDto
            {
                Date = Convert.ToDateTime(reader["MONTH_DATE"]),
                VisitorCount = reader["VISITOR_COUNT"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["VISITOR_COUNT"]),
                NewMembers = reader["NEW_MEMBERS"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["NEW_MEMBERS"])
            });
        }

        if (trends.Count == 0)
            return BuildEmptyVisitorMonths(start, end);

        return trends;
    }

    private static List<VisitorTrendDto> BuildEmptyVisitorMonths(DateTime start, DateTime end)
    {
        var trends = new List<VisitorTrendDto>();
        var currentMonth = new DateTime(start.Year, start.Month, 1);

        while (currentMonth <= end)
        {
            trends.Add(new VisitorTrendDto
            {
                Date = currentMonth,
                VisitorCount = 0,
                NewMembers = 0
            });

            currentMonth = currentMonth.AddMonths(1);
        }

        return trends;
    }

    public async Task<IEnumerable<ArtworkDistributionDto>> GetArtworkDistributionAsync()
    {
        return await GetGlobalArtworkDistributionAsync();
    }

    private async Task<IEnumerable<ArtworkDistributionDto>> GetGlobalArtworkDistributionAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return Enumerable.Empty<ArtworkDistributionDto>();

        var distribution = new List<ArtworkDistributionDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                NVL(col.NAME, 'Unknown Collection') AS CATEGORY,
                COUNT(*) AS ARTWORK_COUNT,
                NVL(SUM(d.ESTIMATED_VALUE), 0) AS TOTAL_VALUE
            FROM ARTWORK_CORE@link_eu c
            JOIN ARTWORK_DETAILS@link_am d
              ON d.ARTWORK_ID = c.ARTWORK_ID
            LEFT JOIN COLLECTION_EU@link_eu col
              ON col.COLLECTION_ID = c.COLLECTION_ID
            GROUP BY NVL(col.NAME, 'Unknown Collection')
            ORDER BY ARTWORK_COUNT DESC";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            distribution.Add(new ArtworkDistributionDto
            {
                Category = reader["CATEGORY"]?.ToString() ?? "Unknown Collection",
                Count = reader["ARTWORK_COUNT"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["ARTWORK_COUNT"]),
                TotalValue = reader["TOTAL_VALUE"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(reader["TOTAL_VALUE"])
            });
        }

        return distribution;
    }

    public async Task<IEnumerable<ExhibitionPerformanceDto>> GetExhibitionPerformanceAsync()
    {
        return await GetGlobalExhibitionPerformanceAsync();
    }

    private async Task<IEnumerable<ExhibitionPerformanceDto>> GetGlobalExhibitionPerformanceAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return Enumerable.Empty<ExhibitionPerformanceDto>();

        var performance = new List<ExhibitionPerformanceDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                e.EXHIBITION_ID,
                e.TITLE,
                e.START_DATE,
                e.END_DATE,
                NVL(ae.ARTWORK_COUNT, 0) AS ARTWORK_COUNT,
                NVL(ae.TOTAL_VALUE, 0) AS TOTAL_VALUE
            FROM (
                SELECT EXHIBITION_ID, TITLE, START_DATE, END_DATE, 'AM' AS SOURCE_REGION
                FROM EXHIBITION_AM@link_am

                UNION ALL

                SELECT EXHIBITION_ID, TITLE, START_DATE, END_DATE, 'EU' AS SOURCE_REGION
                FROM EXHIBITION_EU@link_eu
            ) e
            LEFT JOIN (
                SELECT
                    x.EXHIBITION_ID,
                    x.SOURCE_REGION,
                    COUNT(*) AS ARTWORK_COUNT,
                    NVL(SUM(d.ESTIMATED_VALUE), 0) AS TOTAL_VALUE
                FROM (
                    SELECT ARTWORK_ID, EXHIBITION_ID, 'AM' AS SOURCE_REGION
                    FROM ARTWORK_EXHIBITION_AM@link_am

                    UNION ALL

                    SELECT ARTWORK_ID, EXHIBITION_ID, 'EU' AS SOURCE_REGION
                    FROM ARTWORK_EXHIBITION_EU@link_eu
                ) x
                LEFT JOIN ARTWORK_DETAILS@link_am d
                  ON d.ARTWORK_ID = x.ARTWORK_ID
                GROUP BY x.EXHIBITION_ID, x.SOURCE_REGION
            ) ae
              ON ae.EXHIBITION_ID = e.EXHIBITION_ID
             AND ae.SOURCE_REGION = e.SOURCE_REGION
            ORDER BY e.END_DATE DESC";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var exhibitionId = Convert.ToInt32(reader["EXHIBITION_ID"]);
            var start = Convert.ToDateTime(reader["START_DATE"]);
            var end = Convert.ToDateTime(reader["END_DATE"]);

            var artworkCount = reader["ARTWORK_COUNT"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["ARTWORK_COUNT"]);

            var totalValue = reader["TOTAL_VALUE"] == DBNull.Value
                ? 0
                : Convert.ToDecimal(reader["TOTAL_VALUE"]);

            var durationDays = Math.Max((end - start).Days, 1);
            var baseVisitors = Math.Max(durationDays * 20, 100);

            performance.Add(new ExhibitionPerformanceDto
            {
                ExhibitionId = exhibitionId,
                Title = reader["TITLE"]?.ToString() ?? $"Exhibition #{exhibitionId}",
                StartDate = start,
                EndDate = end,
                ExpectedVisitors = artworkCount > 0 ? artworkCount * 50 : baseVisitors,
                ActualVisitors = artworkCount > 0 ? artworkCount * 45 : (int)(baseVisitors * 0.9),
                PerformanceRatio = 0.90,
                Budget = totalValue > 0 ? totalValue * 0.01m : baseVisitors * 10m
            });
        }

        return performance;
    }

    public async Task<IEnumerable<RevenueDto>> GetRevenueAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return await GetGlobalRevenueAsync(startDate, endDate);
    }

    private async Task<IEnumerable<RevenueDto>> GetGlobalRevenueAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = endDate ?? DateTime.UtcNow;

        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return Enumerable.Empty<RevenueDto>();

        var months = new List<RevenueDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var currentMonth = new DateTime(start.Year, start.Month, 1);

        while (currentMonth <= end)
        {
            var monthEnd = currentMonth.AddMonths(1).AddDays(-1);

            decimal activeInsurance;
            int monthlyLoans;

            await using (var insuranceCommand = connection.CreateCommand())
            {
                insuranceCommand.CommandText = @"
                    SELECT NVL(SUM(i.INSURED_AMOUNT), 0)
                    FROM INSURANCE i
                    LEFT JOIN INSURANCE_POLICY p
                      ON p.POLICY_ID = i.POLICY_ID
                    WHERE p.POLICY_ID IS NULL
                       OR (p.START_DATE <= :monthEnd AND p.END_DATE >= :currentMonth)";

                insuranceCommand.Parameters.Add(new OracleParameter("monthEnd", monthEnd));
                insuranceCommand.Parameters.Add(new OracleParameter("currentMonth", currentMonth));

                var result = await insuranceCommand.ExecuteScalarAsync();
                activeInsurance = result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(result);
            }

            await using (var loanCommand = connection.CreateCommand())
            {
                loanCommand.CommandText = @"
                    SELECT COUNT(*)
                    FROM (
                        SELECT START_DATE FROM LOAN_AM@link_am
                        UNION ALL
                        SELECT START_DATE FROM LOAN_EU@link_eu
                    )
                    WHERE START_DATE >= :currentMonth
                      AND START_DATE <= :monthEnd";

                loanCommand.Parameters.Add(new OracleParameter("currentMonth", currentMonth));
                loanCommand.Parameters.Add(new OracleParameter("monthEnd", monthEnd));

                var result = await loanCommand.ExecuteScalarAsync();
                monthlyLoans = result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }

            months.Add(new RevenueDto
            {
                Period = currentMonth,
                LoanFees = monthlyLoans * 1000m,
                TicketSales = 0,
                MembershipFees = 0,
                TotalRevenue = activeInsurance / 100
            });

            currentMonth = currentMonth.AddMonths(1);
        }

        return months;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        return new DashboardDto
        {
            Kpis = await GetKpisAsync(),
            VisitorTrends = (await GetVisitorTrendsAsync()).ToList(),
            ArtworkDistribution = (await GetArtworkDistributionAsync()).ToList(),
            ExhibitionPerformance = (await GetExhibitionPerformanceAsync()).ToList(),
            RevenueData = (await GetRevenueAsync()).ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }
}