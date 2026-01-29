﻿namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an insurance record for an artwork.
/// Maps to Oracle table: Insurance
/// </summary>
public class Insurance
{
    /// <summary>
    /// Gets or sets the insurance ID (maps to insurance_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the artwork ID (foreign key to Artwork table).
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the policy ID (foreign key to Insurance_Policy table).
    /// </summary>
    public int PolicyId { get; set; }

    /// <summary>
    /// Gets or sets the insured amount.
    /// </summary>
    public decimal InsuredAmount { get; set; }

    /// <summary>
    /// Navigation property to the insured artwork.
    /// </summary>
    public virtual Artwork? Artwork { get; set; }

    /// <summary>
    /// Navigation property to the insurance policy.
    /// </summary>
    public virtual InsurancePolicy? Policy { get; set; }
}
