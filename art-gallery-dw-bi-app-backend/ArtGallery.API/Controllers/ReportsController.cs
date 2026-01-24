using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Reports;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<ApiResponse<KpiDashboardDto>>> GetKpis()
    {
        var result = await _reportService.GetKpisAsync();
        return Ok(ApiResponse<KpiDashboardDto>.SuccessResponse(result));
    }

    [HttpGet("visitor-trends")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VisitorTrendDto>>>> GetVisitorTrends(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _reportService.GetVisitorTrendsAsync(startDate, endDate);
        return Ok(ApiResponse<IEnumerable<VisitorTrendDto>>.SuccessResponse(result));
    }

    [HttpGet("artwork-distribution")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkDistributionDto>>>> GetArtworkDistribution()
    {
        var result = await _reportService.GetArtworkDistributionAsync();
        return Ok(ApiResponse<IEnumerable<ArtworkDistributionDto>>.SuccessResponse(result));
    }

    [HttpGet("exhibition-performance")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExhibitionPerformanceDto>>>> GetExhibitionPerformance()
    {
        var result = await _reportService.GetExhibitionPerformanceAsync();
        return Ok(ApiResponse<IEnumerable<ExhibitionPerformanceDto>>.SuccessResponse(result));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RevenueDto>>>> GetRevenue(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _reportService.GetRevenueAsync(startDate, endDate);
        return Ok(ApiResponse<IEnumerable<RevenueDto>>.SuccessResponse(result));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboard()
    {
        var result = await _reportService.GetDashboardAsync();
        return Ok(ApiResponse<DashboardDto>.SuccessResponse(result));
    }
}
