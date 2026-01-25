namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an artist in the gallery.
/// Maps to Oracle table: Artist
/// </summary>
public class Artist
{
    /// <summary>
    /// Gets or sets the artist ID (maps to artist_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the artist.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nationality of the artist.
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Gets or sets the birth year of the artist.
    /// </summary>
    public int? BirthYear { get; set; }

    /// <summary>
    /// Gets or sets the death year of the artist.
    /// </summary>
    public int? DeathYear { get; set; }

    /// <summary>
    /// Navigation property to the artworks by this artist.
    /// </summary>
    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
