namespace ArtGallery.Application.DTOs.Review;

/// <summary>
/// DTO for creating a new gallery review.
/// Allows creating a new visitor inline or selecting an existing one.
/// </summary>
public class CreateReviewDto
{
    /// <summary>
    /// ID of an existing visitor. If null, NewVisitor must be provided.
    /// </summary>
    public int? VisitorId { get; set; }

    /// <summary>
    /// New visitor data. Used when creating a new visitor with the review.
    /// </summary>
    public CreateVisitorInlineDto? NewVisitor { get; set; }

    /// <summary>
    /// ID of the artwork being reviewed (optional).
    /// </summary>
    public int? ArtworkId { get; set; }

    /// <summary>
    /// ID of the exhibition being reviewed (optional).
    /// </summary>
    public int? ExhibitionId { get; set; }

    /// <summary>
    /// Rating from 1 to 5.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Optional review text.
    /// </summary>
    public string? ReviewText { get; set; }

    /// <summary>
    /// Date of the review.
    /// </summary>
    public DateTime ReviewDate { get; set; }
}

/// <summary>
/// DTO for creating a new visitor inline with a review.
/// </summary>
public class CreateVisitorInlineDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MembershipType { get; set; }
}

/// <summary>
/// DTO for updating a gallery review.
/// </summary>
public class UpdateReviewDto
{
    public int? ArtworkId { get; set; }
    public int? ExhibitionId { get; set; }
    public int? Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime? ReviewDate { get; set; }
}

/// <summary>
/// DTO for gallery review response.
/// </summary>
public class ReviewResponseDto
{
    public int Id { get; set; }
    public int VisitorId { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string? VisitorEmail { get; set; }
    public int? ArtworkId { get; set; }
    public string? ArtworkTitle { get; set; }
    public int? ExhibitionId { get; set; }
    public string? ExhibitionTitle { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime ReviewDate { get; set; }
}

/// <summary>
/// DTO for review statistics.
/// </summary>
public class ReviewStatisticsDto
{
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}
