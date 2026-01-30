﻿namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an artwork in the gallery collection.
/// Maps to Oracle table: Artwork
/// </summary>
public class Artwork
{
    /// <summary>
    /// Gets or sets the artwork ID (maps to artwork_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the artwork.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the artist ID (foreign key to Artist table).
    /// </summary>
    public int ArtistId { get; set; }

    /// <summary>
    /// Gets or sets the year the artwork was created.
    /// </summary>
    public int? YearCreated { get; set; }

    /// <summary>
    /// Gets or sets the medium used to create the artwork.
    /// </summary>
    public string? Medium { get; set; }

    /// <summary>
    /// Gets or sets the collection ID (foreign key to Collection table). 
    /// </summary>
    public int? CollectionId { get; set; }

    /// <summary>
    /// Gets or sets the location ID (foreign key to Location table).
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// Gets or sets the estimated monetary value of the artwork.
    /// </summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>
    /// Navigation property to the associated Artist.
    /// </summary>
    public virtual Artist? Artist { get; set; }

    /// <summary>
    /// Navigation property to the associated Collection.
    /// </summary>
    public virtual Collection? Collection { get; set; }

    /// <summary>
    /// Navigation property to the associated Location.
    /// </summary>
    public virtual Location? Location { get; set; }

    /// <summary>
    /// Gets or sets the exhibition-artwork relationships.
    /// </summary>
    public virtual ICollection<ExhibitionArtwork> ExhibitionArtworks { get; set; } = new List<ExhibitionArtwork>();

    /// <summary>
    /// Gets or sets the loans associated with this artwork.
    /// </summary>
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    /// <summary>
    /// Gets or sets the insurance records for this artwork.
    /// </summary>
    public virtual ICollection<Insurance> Insurances { get; set; } = new List<Insurance>();

    /// <summary>
    /// Gets or sets the restoration records for this artwork.
    /// </summary>
    public virtual ICollection<Restoration> Restorations { get; set; } = new List<Restoration>();
}
