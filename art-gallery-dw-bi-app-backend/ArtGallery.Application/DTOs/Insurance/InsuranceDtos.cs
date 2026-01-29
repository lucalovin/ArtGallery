namespace ArtGallery.Application.DTOs.Insurance;

/// <summary>
/// DTO for creating a new insurance record.
/// </summary>
public class CreateInsuranceDto
{
    public int ArtworkId { get; set; }
    public int PolicyId { get; set; }
    public decimal InsuredAmount { get; set; }
}

/// <summary>
/// DTO for updating an insurance record.
/// </summary>
public class UpdateInsuranceDto
{
    public int? ArtworkId { get; set; }
    public int? PolicyId { get; set; }
    public decimal? InsuredAmount { get; set; }
}

/// <summary>
/// DTO for insurance response.
/// </summary>
public class InsuranceResponseDto
{
    public int Id { get; set; }
    public int ArtworkId { get; set; }
    public string? ArtworkTitle { get; set; }
    public int PolicyId { get; set; }
    public string? PolicyProvider { get; set; }
    public decimal InsuredAmount { get; set; }
}

/// <summary>
/// DTO for insurance policy response.
/// </summary>
public class InsurancePolicyResponseDto
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? TotalCoverageAmount { get; set; }
}

/// <summary>
/// DTO for insurance statistics.
/// </summary>
public class InsuranceStatisticsDto
{
    public int TotalInsurances { get; set; }
    public int TotalPolicies { get; set; }
    public decimal TotalInsuredAmount { get; set; }
    public Dictionary<string, int> ByProvider { get; set; } = new();
}
