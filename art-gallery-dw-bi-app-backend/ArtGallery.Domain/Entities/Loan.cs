﻿namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a loan of an artwork to an external exhibitor.
/// Maps to Oracle table: Loan
/// </summary>
public class Loan
{
    /// <summary>
    /// Gets or sets the loan ID (maps to loan_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the artwork ID (foreign key to Artwork table).
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the exhibitor ID (foreign key to Exhibitor table).
    /// </summary>
    public int ExhibitorId { get; set; }

    /// <summary>
    /// Gets or sets the start date of the loan.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the loan.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets any conditions for the loan.
    /// </summary>
    public string? Conditions { get; set; }

    /// <summary>
    /// Navigation property to the loaned artwork.
    /// </summary>
    public virtual Artwork? Artwork { get; set; }

    /// <summary>
    /// Navigation property to the exhibitor receiving the loan.
    /// </summary>
    public virtual Exhibitor? Exhibitor { get; set; }
}
