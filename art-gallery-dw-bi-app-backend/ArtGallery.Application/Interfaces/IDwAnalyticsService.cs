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
}
