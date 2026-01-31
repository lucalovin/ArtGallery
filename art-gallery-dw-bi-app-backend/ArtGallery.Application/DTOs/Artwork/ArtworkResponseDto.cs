namespace ArtGallery.Application.DTOs.Artwork;

/// <summary>
/// DTO for artwork response data.
/// </summary>
public class ArtworkResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public string? ArtistName { get; set; }
    public int? YearCreated { get; set; }
    public string? Medium { get; set; }
    public int? CollectionId { get; set; }
    public string? CollectionName { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public decimal? EstimatedValue { get; set; }
}

/// <summary>
/// DTO for artwork list item (lighter version for lists).
/// </summary>
public class ArtworkListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public string? ArtistName { get; set; }
    public int? YearCreated { get; set; }
    public string? Medium { get; set; }
    public int? CollectionId { get; set; }
    public string? CollectionName { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public decimal? EstimatedValue { get; set; }
}

/// <summary>
/// DTO for artwork statistics.
/// </summary>
public class ArtworkStatisticsDto
{
    public int TotalArtworks { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public Dictionary<string, int> ByCollection { get; set; } = new();
    public Dictionary<string, int> ByArtist { get; set; } = new();
    public Dictionary<string, int> ByLocation { get; set; } = new();
}
