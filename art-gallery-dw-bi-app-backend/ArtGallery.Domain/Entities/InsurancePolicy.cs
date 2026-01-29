namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an insurance policy.
/// Maps to Oracle table: Insurance_Policy
/// </summary>
public class InsurancePolicy
{
    /// <summary>
    /// Gets or sets the policy ID (maps to policy_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the insurance provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the policy start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the policy end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total coverage amount.
    /// </summary>
    public decimal? TotalCoverageAmount { get; set; }

    /// <summary>
    /// Navigation property to the insurance records under this policy.
    /// </summary>
    public virtual ICollection<Insurance> Insurances { get; set; } = new List<Insurance>();
}
