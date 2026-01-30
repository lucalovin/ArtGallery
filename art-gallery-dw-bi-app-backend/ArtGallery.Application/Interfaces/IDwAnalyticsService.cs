using ArtGallery.Application.DTOs.Reports;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for Data Warehouse analytics queries.
/// </summary>
public interface IDwAnalyticsService
{
    /// <summary>
    /// Gets exhibition summary with visitor counts and revenue.
    /// </summary>
    Task<IEnumerable<ExhibitionSummaryDto>> GetExhibitionSummaryAsync(
        int? year = null, 
        int? artistId = null, 
        int limit = 100);

    /// <summary>
    /// Gets artwork inventory analysis.
    /// </summary>
    Task<IEnumerable<ArtworkInventoryDto>> GetArtworkInventoryAsync(
        int? collectionId = null, 
        int? exhibitionId = null,
        int limit = 100);

    /// <summary>
    /// Gets insurance analysis report.
    /// </summary>
    Task<InsuranceAnalysisDto> GetInsuranceAnalysisAsync(
        DateTime? dateFrom = null, 
        DateTime? dateTo = null);

    /// <summary>
    /// Gets artist performance metrics.
    /// </summary>
    Task<IEnumerable<ArtistPerformanceDto>> GetArtistPerformanceAsync(
        string orderBy = "estimated_value_desc",
        int limit = 50);

    /// <summary>
    /// Gets visitor trends analysis.
    /// </summary>
    Task<VisitorTrendsDto> GetVisitorTrendsAsync(
        DateTime? dateFrom = null, 
        DateTime? dateTo = null,
        string groupBy = "month");

    /// <summary>
    /// Gets revenue breakdown by various dimensions.
    /// </summary>
    Task<RevenueBreakdownDto> GetRevenueBreakdownAsync(
        int? year = null,
        string dimension = "exhibition");

    /// <summary>
    /// Gets KPI dashboard metrics.
    /// </summary>
    Task<KpiDashboardDto> GetKpiDashboardAsync();

    /// <summary>
    /// Gets partition statistics for DW tables.
    /// </summary>
    Task<IEnumerable<PartitionStatDto>> GetPartitionStatsAsync();

    // ============================================================================
    // Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
    // ============================================================================

    /// <summary>
    /// Query 1: Get top N artists by artwork count with total estimated value.
    /// Natural Language: "Show me the top 10 artists with the most artworks in the collection"
    /// </summary>
    /// <param name="topN">Number of top artists to retrieve (default: 10)</param>
    Task<List<ArtistStatisticsDto>> GetTopArtistsByArtworkCountAsync(int topN = 10);

    /// <summary>
    /// Query 2: Get collection value breakdown by art medium and collection type.
    /// Natural Language: "What is the total estimated value broken down by art medium and collection type?"
    /// </summary>
    Task<List<CategoryValueDto>> GetValueByMediumAndCollectionAsync();

    /// <summary>
    /// Query 3: Get monthly exhibition activity metrics for the last N months.
    /// Natural Language: "Analyze exhibition performance: show monthly activity metrics for the past year"
    /// </summary>
    /// <param name="months">Number of months to analyze (default: 12)</param>
    Task<List<MonthlyActivityDto>> GetMonthlyExhibitionActivityAsync(int months = 12);

    /// <summary>
    /// Query 4: Get location/gallery distribution of artworks.
    /// Natural Language: "What is the gallery occupancy rate and distribution of artworks across locations?"
    /// </summary>
    Task<List<LocationDistributionDto>> GetLocationDistributionAsync();

    /// <summary>
    /// Query 5: Get annual exhibition value trends with year-over-year growth.
    /// Natural Language: "Show the trend of exhibition activity: how has the annual total artwork value evolved?"
    /// </summary>
    /// <param name="years">Number of years to analyze (default: 5)</param>
    Task<List<AnnualTrendDto>> GetAnnualExhibitionTrendsAsync(int years = 5);
}
