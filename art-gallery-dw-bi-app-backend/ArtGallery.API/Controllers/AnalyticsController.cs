using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Reports;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

/// <summary>
/// Analytics controller for Data Warehouse reporting and BI queries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IDwAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IDwAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets exhibition summary with visitor counts and revenue metrics.
    /// </summary>
    /// <param name="year">Filter by year (optional)</param>
    /// <param name="artistId">Filter by artist ID (optional)</param>
    /// <param name="limit">Maximum number of results (default: 100)</param>
    [HttpGet("exhibition-summary")]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExhibitionSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExhibitionSummaryDto>>>> GetExhibitionSummary(
        [FromQuery] int? year = null,
        [FromQuery] int? artistId = null,
        [FromQuery] int limit = 100)
    {
        _logger.LogInformation("Getting exhibition summary. Year: {Year}, ArtistId: {ArtistId}, Limit: {Limit}",
            year, artistId, limit);

        var result = await _analyticsService.GetExhibitionSummaryAsync(year, artistId, limit);
        return Ok(ApiResponse<IEnumerable<ExhibitionSummaryDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets artwork inventory analysis with exhibition history.
    /// </summary>
    /// <param name="collectionId">Filter by collection ID (optional)</param>
    /// <param name="exhibitionId">Filter by exhibition ID (optional)</param>
    /// <param name="limit">Maximum number of results (default: 100)</param>
    [HttpGet("artwork-inventory")]
    [ResponseCache(Duration = 300)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtworkInventoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkInventoryDto>>>> GetArtworkInventory(
        [FromQuery] int? collectionId = null,
        [FromQuery] int? exhibitionId = null,
        [FromQuery] int limit = 100)
    {
        var result = await _analyticsService.GetArtworkInventoryAsync(collectionId, exhibitionId, limit);
        return Ok(ApiResponse<IEnumerable<ArtworkInventoryDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets insurance analysis report with coverage breakdown.
    /// </summary>
    /// <param name="dateFrom">Start date for analysis (optional, default: 1 year ago)</param>
    /// <param name="dateTo">End date for analysis (optional, default: today)</param>
    [HttpGet("insurance-analysis")]
    [ResponseCache(Duration = 600)] // Cache for 10 minutes
    [ProducesResponseType(typeof(ApiResponse<InsuranceAnalysisDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InsuranceAnalysisDto>>> GetInsuranceAnalysis(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var result = await _analyticsService.GetInsuranceAnalysisAsync(dateFrom, dateTo);
        return Ok(ApiResponse<InsuranceAnalysisDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets artist performance metrics.
    /// </summary>
    /// <param name="orderBy">Sort order (estimated_value_desc, name, artworks, revenue, visitors)</param>
    /// <param name="limit">Maximum number of results (default: 50)</param>
    [HttpGet("artist-performance")]
    [ResponseCache(Duration = 300)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtistPerformanceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtistPerformanceDto>>>> GetArtistPerformance(
        [FromQuery] string orderBy = "estimated_value_desc",
        [FromQuery] int limit = 50)
    {
        var result = await _analyticsService.GetArtistPerformanceAsync(orderBy, limit);
        return Ok(ApiResponse<IEnumerable<ArtistPerformanceDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets visitor trends analysis over time.
    /// </summary>
    /// <param name="dateFrom">Start date (optional, default: 1 year ago)</param>
    /// <param name="dateTo">End date (optional, default: today)</param>
    /// <param name="groupBy">Time grouping (day, week, month, quarter, year)</param>
    [HttpGet("visitor-trends")]
    [ResponseCache(Duration = 300)]
    [ProducesResponseType(typeof(ApiResponse<VisitorTrendsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<VisitorTrendsDto>>> GetVisitorTrends(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string groupBy = "month")
    {
        var result = await _analyticsService.GetVisitorTrendsAsync(dateFrom, dateTo, groupBy);
        return Ok(ApiResponse<VisitorTrendsDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets revenue breakdown by various dimensions.
    /// </summary>
    /// <param name="year">Year for analysis (optional, default: current year)</param>
    /// <param name="dimension">Dimension for breakdown (exhibition, artist, location)</param>
    [HttpGet("revenue-breakdown")]
    [ResponseCache(Duration = 600)]
    [ProducesResponseType(typeof(ApiResponse<RevenueBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RevenueBreakdownDto>>> GetRevenueBreakdown(
        [FromQuery] int? year = null,
        [FromQuery] string dimension = "exhibition")
    {
        var result = await _analyticsService.GetRevenueBreakdownAsync(year, dimension);
        return Ok(ApiResponse<RevenueBreakdownDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets KPI dashboard metrics for management overview.
    /// </summary>
    [HttpGet("dashboard")]
    [ResponseCache(Duration = 600)]
    [ProducesResponseType(typeof(ApiResponse<KpiDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<KpiDashboardDto>>> GetDashboard()
    {
        var result = await _analyticsService.GetKpiDashboardAsync();
        return Ok(ApiResponse<KpiDashboardDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets partition statistics for the DW fact table.
    /// </summary>
    [HttpGet("partition-stats")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PartitionStatDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PartitionStatDto>>>> GetPartitionStats()
    {
        var result = await _analyticsService.GetPartitionStatsAsync();
        return Ok(ApiResponse<IEnumerable<PartitionStatDto>>.SuccessResponse(result));
    }

    // ============================================================================
    // Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
    // Accessible via /api/reports/analytics/* routes
    // ============================================================================

    /// <summary>
    /// Query 1: Get top N artists by artwork count with total estimated value.
    /// Natural Language: "Show me the top 10 artists with the most artworks in the collection"
    /// </summary>
    /// <param name="topN">Number of top artists to retrieve (default: 10)</param>
    /// <param name="forceRefresh">When true, bypasses cache to get fresh data</param>
    [HttpGet("~/api/reports/analytics/top-artists")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<List<ArtistStatisticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ArtistStatisticsDto>>>> GetTopArtists(
        [FromQuery] int topN = 10)
    {
        _logger.LogInformation("Getting top {TopN} artists by artwork count", topN);
        var result = await _analyticsService.GetTopArtistsByArtworkCountAsync(topN);
        return Ok(ApiResponse<List<ArtistStatisticsDto>>.SuccessResponse(result, 
            $"Retrieved top {topN} artists by artwork count"));
    }

    /// <summary>
    /// Query 2: Get collection value breakdown by art medium and collection type.
    /// Natural Language: "What is the total estimated value broken down by art medium and collection type?"
    /// </summary>
    /// <param name="forceRefresh">When true, bypasses cache to get fresh data</param>
    [HttpGet("~/api/reports/analytics/value-by-category")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryValueDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CategoryValueDto>>>> GetValueByCategory()
    {
        _logger.LogInformation("Getting collection value by medium and collection type");
        var result = await _analyticsService.GetValueByMediumAndCollectionAsync();
        return Ok(ApiResponse<List<CategoryValueDto>>.SuccessResponse(result,
            "Retrieved collection value breakdown by medium and collection"));
    }

    /// <summary>
    /// Query 3: Get monthly exhibition activity metrics for the last N months.
    /// Natural Language: "Analyze exhibition performance: show monthly activity metrics for the past year"
    /// </summary>
    /// <param name="months">Number of months to analyze (default: 12)</param>
    /// <param name="forceRefresh">When true, bypasses cache to get fresh data</param>
    [HttpGet("~/api/reports/analytics/visitor-trends")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<List<MonthlyActivityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<MonthlyActivityDto>>>> GetMonthlyActivity(
        [FromQuery] int months = 12)
    {
        _logger.LogInformation("Getting monthly exhibition activity for last {Months} months", months);
        var result = await _analyticsService.GetMonthlyExhibitionActivityAsync(months);
        return Ok(ApiResponse<List<MonthlyActivityDto>>.SuccessResponse(result,
            $"Retrieved monthly activity for last {months} months"));
    }

    /// <summary>
    /// Query 4: Get location/gallery distribution of artworks.
    /// Natural Language: "What is the gallery occupancy rate and distribution of artworks across locations?"
    /// </summary>
    /// <param name="forceRefresh">When true, bypasses cache to get fresh data</param>
    [HttpGet("~/api/reports/analytics/membership-distribution")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<List<LocationDistributionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LocationDistributionDto>>>> GetLocationDistribution()
    {
        _logger.LogInformation("Getting location/gallery distribution");
        var result = await _analyticsService.GetLocationDistributionAsync();
        return Ok(ApiResponse<List<LocationDistributionDto>>.SuccessResponse(result,
            "Retrieved artwork distribution across locations"));
    }

    /// <summary>
    /// Query 5: Get annual exhibition value trends with year-over-year growth.
    /// Natural Language: "Show the trend of exhibition activity: how has the annual total artwork value evolved?"
    /// </summary>
    /// <param name="years">Number of years to analyze (default: 5)</param>
    /// <param name="forceRefresh">When true, bypasses cache to get fresh data</param>
    [HttpGet("~/api/reports/analytics/acquisition-trends")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<List<AnnualTrendDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AnnualTrendDto>>>> GetAnnualTrends(
        [FromQuery] int years = 5)
    {
        _logger.LogInformation("Getting annual exhibition trends for last {Years} years", years);
        var result = await _analyticsService.GetAnnualExhibitionTrendsAsync(years);
        return Ok(ApiResponse<List<AnnualTrendDto>>.SuccessResponse(result,
            $"Retrieved annual exhibition trends for last {years} years"));
    }
}
