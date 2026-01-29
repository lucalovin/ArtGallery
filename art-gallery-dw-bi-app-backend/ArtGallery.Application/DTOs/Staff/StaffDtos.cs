namespace ArtGallery.Application.DTOs.Staff;

/// <summary>
/// DTO for creating a new staff member.
/// </summary>
public class CreateStaffDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public string? CertificationLevel { get; set; }
}

/// <summary>
/// DTO for updating a staff member.
/// </summary>
public class UpdateStaffDto
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public DateTime? HireDate { get; set; }
    public string? CertificationLevel { get; set; }
}

/// <summary>
/// DTO for staff response.
/// </summary>
public class StaffResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public string? CertificationLevel { get; set; }
}

/// <summary>
/// DTO for staff statistics.
/// </summary>
public class StaffStatisticsDto
{
    public int TotalStaff { get; set; }
    public Dictionary<string, int> ByRole { get; set; } = new();
    public Dictionary<string, int> ByCertificationLevel { get; set; } = new();
}
