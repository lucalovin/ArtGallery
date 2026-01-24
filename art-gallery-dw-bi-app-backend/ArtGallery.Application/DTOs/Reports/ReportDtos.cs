﻿namespace ArtGallery.Application.DTOs.Reports;

// KpiDashboardDto is defined in AnalyticsDtos.cs with extended properties for DW analytics

/// <summary>
/// DTO for visitor trends (OLTP-based).
/// </summary>
public class VisitorTrendDto
{
    public DateTime Date { get; set; }
    public int VisitorCount { get; set; }
    public int NewMembers { get; set; }
}

/// <summary>
/// DTO for artwork distribution.
/// </summary>
public class ArtworkDistributionDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
}

/// <summary>
/// DTO for exhibition performance.
/// </summary>
public class ExhibitionPerformanceDto
{
    public int ExhibitionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? ExpectedVisitors { get; set; }
    public int? ActualVisitors { get; set; }
    public double PerformanceRatio { get; set; }
    public decimal? Budget { get; set; }
}

/// <summary>
/// DTO for revenue data.
/// </summary>
public class RevenueDto
{
    public DateTime Period { get; set; }
    public decimal TicketSales { get; set; }
    public decimal MembershipFees { get; set; }
    public decimal LoanFees { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// DTO for complete dashboard data.
/// </summary>
public class DashboardDto
{
    public KpiDashboardDto Kpis { get; set; } = new();
    public List<VisitorTrendDto> VisitorTrends { get; set; } = new();
    public List<ArtworkDistributionDto> ArtworkDistribution { get; set; } = new();
    public List<ExhibitionPerformanceDto> ExhibitionPerformance { get; set; } = new();
    public List<RevenueDto> RevenueData { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
