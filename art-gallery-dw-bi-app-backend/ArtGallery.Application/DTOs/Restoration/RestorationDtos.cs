namespace ArtGallery.Application.DTOs.Restoration;

/// <summary>
/// DTO for creating a new restoration record.
/// </summary>
public class CreateRestorationDto
{
    public int ArtworkId { get; set; }
    public int StaffId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for updating a restoration record.
/// </summary>
public class UpdateRestorationDto
{
    public int? ArtworkId { get; set; }
    public int? StaffId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for restoration response.
/// </summary>
public class RestorationResponseDto
{
    public int Id { get; set; }
    public int ArtworkId { get; set; }
    public string? ArtworkTitle { get; set; }
    public int StaffId { get; set; }
    public string? StaffName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for restoration statistics.
/// </summary>
public class RestorationStatisticsDto
{
    public int TotalRestorations { get; set; }
    public int InProgressRestorations { get; set; }
    public int CompletedRestorations { get; set; }
    public Dictionary<string, int> ByStaff { get; set; } = new();
}
