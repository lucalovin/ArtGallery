namespace ArtGallery.Application.DTOs.Exhibition;

/// <summary>
/// DTO for creating a new exhibition.
/// </summary>
public class CreateExhibitionDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExhibitorId { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for updating an exhibition.
/// </summary>
public class UpdateExhibitionDto
{
    public string? Title { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ExhibitorId { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for exhibition response.
/// </summary>
public class ExhibitionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ExhibitorId { get; set; }
    public string? ExhibitorName { get; set; }
    public string? Description { get; set; }
    public int ArtworkCount { get; set; }
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
    public int ArtworkId { get; set; }
    public int ExhibitionId { get; set; }
    public string? ArtworkTitle { get; set; }
    public string? ArtistName { get; set; }
    public string? PositionInGallery { get; set; }
    public string? FeaturedStatus { get; set; }
}
