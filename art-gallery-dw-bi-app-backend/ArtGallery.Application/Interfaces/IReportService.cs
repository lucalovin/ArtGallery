using ArtGallery.Application.DTOs.Reports;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for reports and analytics.
/// </summary>
public interface IReportService
{
    Task<KpiDashboardDto> GetKpisAsync();
    Task<IEnumerable<VisitorTrendDto>> GetVisitorTrendsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ArtworkDistributionDto>> GetArtworkDistributionAsync();
    Task<IEnumerable<ExhibitionPerformanceDto>> GetExhibitionPerformanceAsync();
    Task<IEnumerable<RevenueDto>> GetRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<DashboardDto> GetDashboardAsync();
}
