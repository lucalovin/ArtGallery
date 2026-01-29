namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between exhibitions and artworks.
/// Maps to Oracle table: Artwork_Exhibition
/// </summary>
public class ExhibitionArtwork
{
    /// <summary>
    /// Gets or sets the artwork ID.
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the exhibition ID.
    /// </summary>
    public int ExhibitionId { get; set; }

    /// <summary>
    /// Gets or sets the position in the gallery.
    /// </summary>
    public string? PositionInGallery { get; set; }

    /// <summary>
    /// Gets or sets the featured status.
    /// </summary>
    public string? FeaturedStatus { get; set; }

    /// <summary>
    /// Navigation property to the artwork.
    /// </summary>
    public virtual Artwork? Artwork { get; set; }

    /// <summary>
    /// Navigation property to the exhibition.
    /// </summary>
    public virtual Exhibition? Exhibition { get; set; }
}
