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
    /// Gets or sets the artist who created the artwork.
    /// </summary>
    public string? Artist { get; set; }

    /// <summary>
    /// Gets or sets the year the artwork was created.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the medium used.
    /// </summary>
    public string? Medium { get; set; }

    /// <summary>
    /// Gets or sets the dimensions.
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the collection type.
    /// </summary>
    public string? Collection { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the estimated value.
    /// </summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public List<string>? Tags { get; set; }
}
