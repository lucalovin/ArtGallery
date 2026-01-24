using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an artwork in the gallery collection.
/// </summary>
public class Artwork : BaseEntity
{
    /// <summary>
    /// Gets or sets the title of the artwork.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the artist who created the artwork.
    /// </summary>
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the year the artwork was created.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the medium used to create the artwork (e.g., Oil on canvas).
    /// </summary>
    public string Medium { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dimensions of the artwork.
    /// </summary>
    public string Dimensions { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the artwork.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL of the artwork's image.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the collection type (e.g., Permanent, Temporary, Loan).
    /// </summary>
    public string Collection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the artwork.
    /// </summary>
    public string Status { get; set; } = "Available";

    /// <summary>
    /// Gets or sets the estimated monetary value of the artwork.
    /// </summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>
    /// Gets or sets the current location of the artwork within the gallery.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the date when the artwork was acquired.
    /// </summary>
    public DateTime AcquisitionDate { get; set; }

    /// <summary>
    /// Gets or sets the method by which the artwork was acquired (e.g., Purchase, Donation, Bequest).
    /// </summary>
    public string? AcquisitionMethod { get; set; }

    /// <summary>
    /// Gets or sets the provenance (ownership history) of the artwork.
    /// </summary>
    public string? Provenance { get; set; }

    /// <summary>
    /// Gets or sets the current condition of the artwork.
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the artwork for categorization.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the exhibition-artwork relationships.
    /// </summary>
    public virtual ICollection<ExhibitionArtwork> ExhibitionArtworks { get; set; } = new List<ExhibitionArtwork>();

    /// <summary>
    /// Gets or sets the loans associated with this artwork.
    /// </summary>
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    /// <summary>
    /// Gets or sets the insurance policies for this artwork.
    /// </summary>
    public virtual ICollection<Insurance> Insurances { get; set; } = new List<Insurance>();

    /// <summary>
    /// Gets or sets the restoration records for this artwork.
    /// </summary>
    public virtual ICollection<Restoration> Restorations { get; set; } = new List<Restoration>();
}
