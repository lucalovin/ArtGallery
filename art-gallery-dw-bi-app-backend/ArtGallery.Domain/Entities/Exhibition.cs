using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an exhibition in the gallery.
/// </summary>
public class Exhibition : BaseEntity
{
    /// <summary>
    /// Gets or sets the title of the exhibition.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the exhibition.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the start date of the exhibition.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the exhibition.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the current status of the exhibition.
    /// </summary>
    public string Status { get; set; } = "Planning";

    /// <summary>
    /// Gets or sets the location of the exhibition within the gallery.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the curator responsible for the exhibition.
    /// </summary>
    public string? Curator { get; set; }

    /// <summary>
    /// Gets or sets the URL of the exhibition's promotional image.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the budget allocated for the exhibition.
    /// </summary>
    public decimal? Budget { get; set; }

    /// <summary>
    /// Gets or sets the expected number of visitors.
    /// </summary>
    public int? ExpectedVisitors { get; set; }

    /// <summary>
    /// Gets or sets the actual number of visitors.
    /// </summary>
    public int? ActualVisitors { get; set; }

    /// <summary>
    /// Gets or sets the exhibition-artwork relationships.
    /// </summary>
    public virtual ICollection<ExhibitionArtwork> ExhibitionArtworks { get; set; } = new List<ExhibitionArtwork>();
}
