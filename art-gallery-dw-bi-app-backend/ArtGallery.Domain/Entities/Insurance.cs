using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an insurance policy for an artwork.
/// </summary>
public class Insurance : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the insured artwork.
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the insured artwork.
    /// </summary>
    public virtual Artwork Artwork { get; set; } = null!;

    /// <summary>
    /// Gets or sets the insurance provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the policy number.
    /// </summary>
    public string PolicyNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the coverage amount.
    /// </summary>
    public decimal CoverageAmount { get; set; }

    /// <summary>
    /// Gets or sets the premium amount.
    /// </summary>
    public decimal Premium { get; set; }

    /// <summary>
    /// Gets or sets the policy start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the policy end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the policy status.
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Gets or sets the type of coverage (e.g., All-Risk, Named Perils).
    /// </summary>
    public string? CoverageType { get; set; }

    /// <summary>
    /// Gets or sets any additional notes about the policy.
    /// </summary>
    public string? Notes { get; set; }
}
