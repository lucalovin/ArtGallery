namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between exhibitions and artworks.
/// </summary>
public class ExhibitionArtwork
{
    /// <summary>
    /// Gets or sets the exhibition ID.
    /// </summary>
    public int ExhibitionId { get; set; }

    /// <summary>
    /// Gets or sets the associated exhibition.
    /// </summary>
    public virtual Exhibition Exhibition { get; set; } = null!;

    /// <summary>
    /// Gets or sets the artwork ID.
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the associated artwork.
    /// </summary>
    public virtual Artwork Artwork { get; set; } = null!;

    /// <summary>
    /// Gets or sets the display order of the artwork in the exhibition.
    /// </summary>
    public int? DisplayOrder { get; set; }
}
