﻿namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a restoration project for an artwork.
/// Maps to Oracle table: Restoration
/// </summary>
public class Restoration
{
    /// <summary>
    /// Gets or sets the restoration ID (maps to restoration_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the artwork ID (foreign key to Artwork table).
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the staff ID (foreign key to Staff table).
    /// </summary>
    public int StaffId { get; set; }

    /// <summary>
    /// Gets or sets the start date of the restoration.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the restoration.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the description of the restoration work.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to the artwork being restored.
    /// </summary>
    public virtual Artwork? Artwork { get; set; }

    /// <summary>
    /// Navigation property to the staff member performing the restoration.
    /// </summary>
    public virtual Staff? Staff { get; set; }
}
