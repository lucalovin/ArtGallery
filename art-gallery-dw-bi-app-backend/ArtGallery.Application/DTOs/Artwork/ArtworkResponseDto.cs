namespace ArtGallery.Application.DTOs.Artwork;

/// <summary>
/// DTO for artwork response data.
/// </summary>
public class ArtworkResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Medium { get; set; } = string.Empty;
    public string Dimensions { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Collection { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? EstimatedValue { get; set; }
    public string? Location { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public string? AcquisitionMethod { get; set; }
    public string? Provenance { get; set; }
    public string? Condition { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for artwork list item (lighter version for lists).
/// </summary>
public class ArtworkListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Medium { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? EstimatedValue { get; set; }
    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
}

/// <summary>
/// DTO for artwork statistics.
/// </summary>
public class ArtworkStatisticsDto
{
    public int TotalArtworks { get; set; }
    public int AvailableArtworks { get; set; }
    public int OnDisplayArtworks { get; set; }
    public int OnLoanArtworks { get; set; }
    public int UnderRestorationArtworks { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public Dictionary<string, int> ByCollection { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
}
