namespace ArtGallery.Application.DTOs.Reports;

/// <summary>
/// Exhibition summary with visitor and revenue metrics.
/// </summary>
public class ExhibitionSummaryDto
{
    public int ExhibitionKey { get; set; }
    public string ExhibitionName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DurationDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalVisitors { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TicketRevenue { get; set; }
    public decimal MerchandiseRevenue { get; set; }
    public int ArtworkCount { get; set; }
    public decimal AverageVisitDuration { get; set; }
    public decimal RevenuePerVisitor { get; set; }
}

/// <summary>
/// Artwork inventory analysis.
/// </summary>
public class ArtworkInventoryDto
{
    public int ArtworkKey { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public int? CreationYear { get; set; }
    public string? Medium { get; set; }
    public string? CollectionType { get; set; }
    public string? Status { get; set; }
    public decimal? EstimatedValue { get; set; }
    public decimal? InsuranceValue { get; set; }
    public int ExhibitionCount { get; set; }
    public DateTime? LastExhibitionDate { get; set; }
}

/// <summary>
/// Insurance analysis report.
/// </summary>
public class InsuranceAnalysisDto
{
    public decimal TotalCoverageAmount { get; set; }
    public decimal TotalPremiumAmount { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiredPolicies { get; set; }
    public int ExpiringThisMonth { get; set; }
    public decimal AverageCoveragePerArtwork { get; set; }
    public List<InsuranceByProviderDto> ByProvider { get; set; } = new();
    public List<InsuranceByCoverageTypeDto> ByCoverageType { get; set; } = new();
}

public class InsuranceByProviderDto
{
    public string Provider { get; set; } = string.Empty;
    public int PolicyCount { get; set; }
    public decimal TotalCoverage { get; set; }
    public decimal TotalPremium { get; set; }
}

public class InsuranceByCoverageTypeDto
{
    public string CoverageType { get; set; } = string.Empty;
    public int PolicyCount { get; set; }
    public decimal TotalCoverage { get; set; }
}

/// <summary>
/// Artist performance metrics.
/// </summary>
public class ArtistPerformanceDto
{
    public int ArtistKey { get; set; }
    public string ArtistName { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public string? ArtMovement { get; set; }
    public int TotalArtworks { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public decimal AverageArtworkValue { get; set; }
    public int ExhibitionAppearances { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalVisitors { get; set; }
}

/// <summary>
/// Visitor trends analysis.
/// </summary>
public class VisitorTrendsDto
{
    public int TotalVisitors { get; set; }
    public decimal AverageVisitDuration { get; set; }
    public decimal AverageSpendPerVisit { get; set; }
    public string PeakDay { get; set; } = string.Empty;
    public string PeakMonth { get; set; } = string.Empty;
    public List<VisitorTrendDataPoint> TrendData { get; set; } = new();
    public Dictionary<string, int> ByMembershipType { get; set; } = new();
    public Dictionary<string, int> ByAgeGroup { get; set; } = new();
}

public class VisitorTrendDataPoint
{
    public string Period { get; set; } = string.Empty;
    public int VisitorCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageSpend { get; set; }
}

/// <summary>
/// Revenue breakdown analysis.
/// </summary>
public class RevenueBreakdownDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TicketRevenue { get; set; }
    public decimal MerchandiseRevenue { get; set; }
    public decimal OtherRevenue { get; set; }
    public List<RevenueDimensionDto> BreakdownByDimension { get; set; } = new();
    public List<RevenueTimeSeriesDto> TimeSeries { get; set; } = new();
}

public class RevenueDimensionDto
{
    public string DimensionValue { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class RevenueTimeSeriesDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal TicketRevenue { get; set; }
    public decimal MerchandiseRevenue { get; set; }
}

/// <summary>
/// KPI Dashboard metrics.
/// </summary>
public class KpiDashboardDto
{
    public DateTime AsOf { get; set; } = DateTime.UtcNow;
    
    // Backward compatibility aliases
    public DateTime GeneratedAt { get => AsOf; set => AsOf = value; }
    
    // Overall metrics
    public int TotalArtworks { get; set; }
    public int TotalArtists { get; set; }
    public int TotalExhibitions { get; set; }
    public int ActiveExhibitions { get; set; }
    public int TotalVisitorsYtd { get; set; }
    public decimal TotalRevenueYtd { get; set; }
    
    // Backward compatibility - maps to YTD
    public int TotalVisitors { get => TotalVisitorsYtd; set => TotalVisitorsYtd = value; }
    public int TotalStaff { get; set; }
    public int ActiveLoans { get; set; }
    public int ArtworksUnderRestoration { get => ArtworksInRestoration; set => ArtworksInRestoration = value; }
    public decimal TotalInsuranceCoverage { get => TotalInsuranceValue; set => TotalInsuranceValue = value; }
    
    // Trend indicators (vs previous period)
    public decimal VisitorTrendPercent { get; set; }
    public decimal RevenueTrendPercent { get; set; }
    public decimal ArtworkValueTrendPercent { get; set; }
    
    // Collection metrics
    public decimal TotalCollectionValue { get; set; }
    public decimal TotalInsuranceValue { get; set; }
    public int ArtworksOnDisplay { get; set; }
    public int ArtworksInStorage { get; set; }
    public int ArtworksOnLoan { get; set; }
    public int ArtworksInRestoration { get; set; }
    
    // Upcoming events
    public int UpcomingExhibitions { get; set; }
    public int ExpiringInsurancePolicies { get; set; }
    public int PendingRestorations { get; set; }
}

/// <summary>
/// Partition statistics for DW tables.
/// </summary>
public class PartitionStatDto
{
    public string TableName { get; set; } = string.Empty;
    public string PartitionName { get; set; } = string.Empty;
    public int PartitionYear { get; set; }
    public long RowCount { get; set; }
    public decimal SizeMb { get; set; }
    public DateTime? LastAnalyzed { get; set; }
}

// ============================================================================
// Module 1 & 2, Requirement 10: Five Natural Language Analytical Queries
// ============================================================================

/// <summary>
/// Query 1: Top artists by artwork count with total estimated value.
/// Natural Language: "Show me the top 10 artists with the most artworks in the collection"
/// </summary>
public class ArtistStatisticsDto
{
    public string ArtistName { get; set; } = string.Empty;
    public int ArtworkCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AverageValue { get; set; }
}

/// <summary>
/// Query 2: Collection value breakdown by art medium and collection type.
/// Natural Language: "What is the total estimated value of the collection broken down by art medium and collection type?"
/// </summary>
public class CategoryValueDto
{
    public string? MediumType { get; set; }
    public string? CollectionName { get; set; }
    public int ArtworkCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AverageValue { get; set; }
}

/// <summary>
/// Query 3: Monthly exhibition activity metrics.
/// Natural Language: "Analyze exhibition performance: show monthly activity metrics for the past year"
/// </summary>
public class MonthlyActivityDto
{
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int ExhibitionCount { get; set; }
    public int ArtworksExhibited { get; set; }
    public decimal TotalArtworkValue { get; set; }
    public decimal? AverageRating { get; set; }
}

/// <summary>
/// Query 4: Location/gallery distribution of artworks.
/// Natural Language: "What is the gallery occupancy rate and distribution of artworks across different locations?"
/// </summary>
public class LocationDistributionDto
{
    public string LocationName { get; set; } = string.Empty;
    public string? GalleryRoom { get; set; }
    public string? LocationType { get; set; }
    public int ArtworksCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Query 5: Annual exhibition value trends with year-over-year growth.
/// Natural Language: "Show the trend of exhibition activity: how has the annual total artwork value evolved?"
/// </summary>
public class AnnualTrendDto
{
    public int Year { get; set; }
    public int ExhibitionsCount { get; set; }
    public int ArtworksCount { get; set; }
    public decimal TotalArtworkValue { get; set; }
    public decimal AverageArtworkValue { get; set; }
    public decimal? PreviousYearValue { get; set; }
    public decimal? YoyGrowthRate { get; set; }
}
