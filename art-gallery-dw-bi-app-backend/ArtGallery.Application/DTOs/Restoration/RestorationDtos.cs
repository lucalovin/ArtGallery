namespace ArtGallery.Application.DTOs.Restoration;

/// <summary>
/// DTO for creating a new restoration record.
/// </summary>
public class CreateRestorationDto
{
    public int ArtworkId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Conservator { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? ConditionBefore { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a restoration record.
/// </summary>
public class UpdateRestorationDto
{
    public string? Type { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? Conservator { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? ConditionBefore { get; set; }
    public string? ConditionAfter { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for restoration response.
/// </summary>
public class RestorationResponseDto
{
    public int Id { get; set; }
    public int ArtworkId { get; set; }
    public string ArtworkTitle { get; set; } = string.Empty;
    public string ArtworkArtist { get; set; } = string.Empty;
    public string? ArtworkImageUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Conservator { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? ConditionBefore { get; set; }
    public string? ConditionAfter { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for restoration statistics.
/// </summary>
public class RestorationStatisticsDto
{
    public int TotalRestorations { get; set; }
    public int InProgressRestorations { get; set; }
    public int CompletedRestorations { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByType { get; set; } = new();
}
