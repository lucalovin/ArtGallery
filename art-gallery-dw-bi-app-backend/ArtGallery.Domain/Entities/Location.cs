namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a location in the gallery.
/// Maps to Oracle table: Location
/// </summary>
public class Location
{
    /// <summary>
    /// Gets or sets the location ID (maps to location_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the location.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gallery room identifier.
    /// </summary>
    public string? GalleryRoom { get; set; }

    /// <summary>
    /// Gets or sets the type of location (e.g., Exhibit, Storage).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the capacity of the location.
    /// </summary>
    public int? Capacity { get; set; }

    /// <summary>
    /// Navigation property to the artworks at this location.
    /// </summary>
    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
