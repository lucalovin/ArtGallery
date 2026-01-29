﻿namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an exhibition in the gallery.
/// Maps to Oracle table: Exhibition
/// </summary>
public class Exhibition
{
    /// <summary>
    /// Gets or sets the exhibition ID (maps to exhibition_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the exhibition.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date of the exhibition.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the exhibition.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the exhibitor ID (foreign key to Exhibitor table).
    /// </summary>
    public int ExhibitorId { get; set; }

    /// <summary>
    /// Gets or sets the description of the exhibition.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to the exhibitor.
    /// </summary>
    public virtual Exhibitor? Exhibitor { get; set; }

    /// <summary>
    /// Gets or sets the exhibition-artwork relationships.
    /// </summary>
    public virtual ICollection<ExhibitionArtwork> ExhibitionArtworks { get; set; } = new List<ExhibitionArtwork>();
}
