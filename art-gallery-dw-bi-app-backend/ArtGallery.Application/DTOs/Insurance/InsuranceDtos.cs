namespace ArtGallery.Application.DTOs.Insurance;

/// <summary>
/// DTO for creating a new insurance policy.
/// </summary>
public class CreateInsuranceDto
{
    public int ArtworkId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public decimal Premium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CoverageType { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an insurance policy.
/// </summary>
public class UpdateInsuranceDto
{
    public string? Provider { get; set; }
    public string? PolicyNumber { get; set; }
    public decimal? CoverageAmount { get; set; }
    public decimal? Premium { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? CoverageType { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for insurance response.
/// </summary>
public class InsuranceResponseDto
{
    public int Id { get; set; }
    public int ArtworkId { get; set; }
    public string ArtworkTitle { get; set; } = string.Empty;
    public string ArtworkArtist { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public decimal Premium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CoverageType { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for insurance statistics.
/// </summary>
public class InsuranceStatisticsDto
{
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiringPolicies { get; set; }
    public decimal TotalCoverageAmount { get; set; }
    public decimal TotalPremiums { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByProvider { get; set; } = new();
}
