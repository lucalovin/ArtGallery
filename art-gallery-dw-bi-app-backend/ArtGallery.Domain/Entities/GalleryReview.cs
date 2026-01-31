namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a gallery review made by a visitor.
/// Maps to Oracle table: GALLERY_REVIEW
/// </summary>
public class GalleryReview
{
    /// <summary>
    /// Gets or sets the review ID (maps to REVIEW_ID in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the visitor ID who made the review.
    /// </summary>
    public int VisitorId { get; set; }

    /// <summary>
    /// Gets or sets the artwork ID being reviewed (optional).
    /// </summary>
    public int? ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the exhibition ID being reviewed (optional).
    /// </summary>
    public int? ExhibitionId { get; set; }

    /// <summary>
    /// Gets or sets the rating (1-5).
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Gets or sets the review text.
    /// </summary>
    public string? ReviewText { get; set; }

    /// <summary>
    /// Gets or sets the review date.
    /// </summary>
    public DateTime ReviewDate { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the visitor who made the review.
    /// </summary>
    public virtual Visitor? Visitor { get; set; }

    /// <summary>
    /// Gets or sets the artwork being reviewed.
    /// </summary>
    public virtual Artwork? Artwork { get; set; }

    /// <summary>
    /// Gets or sets the exhibition being reviewed.
    /// </summary>
    public virtual Exhibition? Exhibition { get; set; }
}
