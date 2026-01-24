using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a loan of an artwork to an external institution.
/// </summary>
public class Loan : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the loaned artwork.
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the artwork being loaned.
    /// </summary>
    public virtual Artwork Artwork { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the borrowing institution or individual.
    /// </summary>
    public string BorrowerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of borrower (e.g., Museum, Gallery, Private Collector).
    /// </summary>
    public string? BorrowerType { get; set; }

    /// <summary>
    /// Gets or sets the contact information for the borrower.
    /// </summary>
    public string? BorrowerContact { get; set; }

    /// <summary>
    /// Gets or sets the borrower's address.
    /// </summary>
    public string? BorrowerAddress { get; set; }

    /// <summary>
    /// Gets or sets the start date of the loan.
    /// </summary>
    public DateTime LoanStartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the loan.
    /// </summary>
    public DateTime LoanEndDate { get; set; }

    /// <summary>
    /// Gets or sets the current status of the loan.
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets the insurance value for the loan period.
    /// </summary>
    public decimal? InsuranceValue { get; set; }

    /// <summary>
    /// Gets or sets the insurance provider for the loan.
    /// </summary>
    public string? InsuranceProvider { get; set; }

    /// <summary>
    /// Gets or sets the insurance policy number.
    /// </summary>
    public string? InsurancePolicyNumber { get; set; }

    /// <summary>
    /// Gets or sets the fee charged for the loan.
    /// </summary>
    public decimal? LoanFee { get; set; }

    /// <summary>
    /// Gets or sets the purpose of the loan.
    /// </summary>
    public string? Purpose { get; set; }

    /// <summary>
    /// Gets or sets the condition of the artwork when loaned.
    /// </summary>
    public string? ConditionOnLoan { get; set; }

    /// <summary>
    /// Gets or sets the condition of the artwork when returned.
    /// </summary>
    public string? ConditionOnReturn { get; set; }

    /// <summary>
    /// Gets or sets any additional notes about the loan.
    /// </summary>
    public string? Notes { get; set; }
}
