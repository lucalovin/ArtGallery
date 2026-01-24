namespace ArtGallery.Application.DTOs.Loan;

/// <summary>
/// DTO for creating a new loan.
/// </summary>
public class CreateLoanDto
{
    public int ArtworkId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public string? BorrowerType { get; set; }
    public string? BorrowerContact { get; set; }
    public string? BorrowerAddress { get; set; }
    public DateTime LoanStartDate { get; set; }
    public DateTime LoanEndDate { get; set; }
    public decimal? InsuranceValue { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public decimal? LoanFee { get; set; }
    public string? Purpose { get; set; }
    public string? ConditionOnLoan { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a loan.
/// </summary>
public class UpdateLoanDto
{
    public string? BorrowerName { get; set; }
    public string? BorrowerType { get; set; }
    public string? BorrowerContact { get; set; }
    public string? BorrowerAddress { get; set; }
    public DateTime? LoanStartDate { get; set; }
    public DateTime? LoanEndDate { get; set; }
    public string? Status { get; set; }
    public decimal? InsuranceValue { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public decimal? LoanFee { get; set; }
    public string? Purpose { get; set; }
    public string? ConditionOnLoan { get; set; }
    public string? ConditionOnReturn { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for loan response.
/// </summary>
public class LoanResponseDto
{
    public int Id { get; set; }
    public int ArtworkId { get; set; }
    public string ArtworkTitle { get; set; } = string.Empty;
    public string ArtworkArtist { get; set; } = string.Empty;
    public string? ArtworkImageUrl { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public string? BorrowerType { get; set; }
    public string? BorrowerContact { get; set; }
    public string? BorrowerAddress { get; set; }
    public DateTime LoanStartDate { get; set; }
    public DateTime LoanEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? InsuranceValue { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public decimal? LoanFee { get; set; }
    public string? Purpose { get; set; }
    public string? ConditionOnLoan { get; set; }
    public string? ConditionOnReturn { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for loan statistics.
/// </summary>
public class LoanStatisticsDto
{
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public decimal TotalInsuranceValue { get; set; }
    public decimal TotalLoanFees { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
}
