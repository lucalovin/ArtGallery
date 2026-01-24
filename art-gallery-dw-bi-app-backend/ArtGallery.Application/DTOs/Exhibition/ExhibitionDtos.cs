namespace ArtGallery.Application.DTOs.Exhibition;

/// <summary>
/// DTO for creating a new exhibition.
/// </summary>
public class CreateExhibitionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? Curator { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Budget { get; set; }
    public int? ExpectedVisitors { get; set; }
}

/// <summary>
/// DTO for updating an exhibition.
/// </summary>
public class UpdateExhibitionDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
    public string? Curator { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Budget { get; set; }
    public int? ExpectedVisitors { get; set; }
    public int? ActualVisitors { get; set; }
}

/// <summary>
/// DTO for exhibition response.
/// </summary>
public class ExhibitionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Curator { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Budget { get; set; }
    public int? ExpectedVisitors { get; set; }
    public int? ActualVisitors { get; set; }
    public int ArtworkCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for exhibition with artworks.
/// </summary>
public class ExhibitionDetailDto : ExhibitionResponseDto
{
    public List<ExhibitionArtworkDto> Artworks { get; set; } = new();
}

/// <summary>
/// DTO for artwork in exhibition.
/// </summary>
public class ExhibitionArtworkDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? DisplayOrder { get; set; }
}
