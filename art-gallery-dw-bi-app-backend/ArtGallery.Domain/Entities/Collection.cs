namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a collection in the gallery.
/// Maps to Oracle table: Collection
/// </summary>
public class Collection
{
    /// <summary>
    /// Gets or sets the collection ID (maps to collection_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the collection.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the collection.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the date the collection was created.
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Navigation property to the artworks in this collection.
    /// </summary>
    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
