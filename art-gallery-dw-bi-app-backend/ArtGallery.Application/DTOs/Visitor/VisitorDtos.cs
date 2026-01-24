namespace ArtGallery.Application.DTOs.Visitor;

/// <summary>
/// DTO for creating a new visitor.
/// </summary>
public class CreateVisitorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string MembershipType { get; set; } = "None";
    public DateTime? MembershipExpiry { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a visitor.
/// </summary>
public class UpdateVisitorDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MembershipType { get; set; }
    public DateTime? MembershipExpiry { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for visitor response.
/// </summary>
public class VisitorResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string MembershipType { get; set; } = string.Empty;
    public DateTime? MembershipExpiry { get; set; }
    public int TotalVisits { get; set; }
    public DateTime? LastVisit { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for visitor statistics.
/// </summary>
public class VisitorStatisticsDto
{
    public int TotalVisitors { get; set; }
    public int TotalMembers { get; set; }
    public int NewVisitorsThisMonth { get; set; }
    public Dictionary<string, int> ByMembershipType { get; set; } = new();
    public Dictionary<string, int> ByCountry { get; set; } = new();
}
