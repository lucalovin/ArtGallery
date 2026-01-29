namespace ArtGallery.Application.DTOs.Loan;

/// <summary>
/// DTO for creating a new loan.
/// </summary>
public class CreateLoanDto
{
    public int ArtworkId { get; set; }
    public int ExhibitorId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Conditions { get; set; }
}

/// <summary>
/// DTO for updating a loan.
/// </summary>
public class UpdateLoanDto
{
    public int? ArtworkId { get; set; }
    public int? ExhibitorId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Conditions { get; set; }
}

/// <summary>
/// DTO for loan response.
/// </summary>
public class LoanResponseDto
{
    public int Id { get; set; }
    public int ArtworkId { get; set; }
    public string? ArtworkTitle { get; set; }
    public int ExhibitorId { get; set; }
    public string? ExhibitorName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Conditions { get; set; }
}

/// <summary>
/// DTO for loan statistics.
/// </summary>
public class LoanStatisticsDto
{
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public Dictionary<string, int> ByExhibitor { get; set; } = new();
}
