namespace ArtGallery.Application.DTOs.Artwork;

/// <summary>
/// DTO for updating an existing artwork.
/// </summary>
public class UpdateArtworkDto
{
    /// <summary>
    /// Gets or sets the title of the artwork.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the artist ID (foreign key).
    /// </summary>
    public int? ArtistId { get; set; }

    /// <summary>
    /// Gets or sets the year the artwork was created.
    /// </summary>
    public int? YearCreated { get; set; }

    /// <summary>
    /// Gets or sets the medium used.
    /// </summary>
    public string? Medium { get; set; }

    /// <summary>
    /// Gets or sets the collection ID (foreign key).
    /// </summary>
    public int? CollectionId { get; set; }

    /// <summary>
    /// Gets or sets the location ID (foreign key).
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// Gets or sets the estimated value.
    /// </summary>
    public decimal? EstimatedValue { get; set; }
}
