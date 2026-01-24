namespace ArtGallery.Application.DTOs.Staff;

/// <summary>
/// DTO for creating a new staff member.
/// </summary>
public class CreateStaffDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal? Salary { get; set; }
    public string? ImageUrl { get; set; }
    public string? Bio { get; set; }
}

/// <summary>
/// DTO for updating a staff member.
/// </summary>
public class UpdateStaffDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public string? Status { get; set; }
    public string? ImageUrl { get; set; }
    public string? Bio { get; set; }
}

/// <summary>
/// DTO for staff response.
/// </summary>
public class StaffResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public decimal? Salary { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for staff statistics.
/// </summary>
public class StaffStatisticsDto
{
    public int TotalStaff { get; set; }
    public int ActiveStaff { get; set; }
    public Dictionary<string, int> ByDepartment { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
}
