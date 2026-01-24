namespace ArtGallery.Application.DTOs.Artwork;

/// <summary>
/// DTO for creating a new artwork.
/// </summary>
public class CreateArtworkDto
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
    /// Gets or sets the medium used to create the artwork.
    /// </summary>
    public string Medium { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dimensions of the artwork.
    /// </summary>
    public string Dimensions { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the artwork.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL of the artwork's image.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the collection type.
    /// </summary>
    public string Collection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the artwork.
    /// </summary>
    public string Status { get; set; } = "Available";

    /// <summary>
    /// Gets or sets the estimated value.
    /// </summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the acquisition date.
    /// </summary>
    public DateTime AcquisitionDate { get; set; }

    /// <summary>
    /// Gets or sets the acquisition method.
    /// </summary>
    public string? AcquisitionMethod { get; set; }

    /// <summary>
    /// Gets or sets the provenance.
    /// </summary>
    public string? Provenance { get; set; }

    /// <summary>
    /// Gets or sets the condition.
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Gets or sets the tags.
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
