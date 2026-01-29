namespace ArtGallery.Application.DTOs.Visitor;

/// <summary>
/// DTO for creating a new visitor.
/// </summary>
public class CreateVisitorDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MembershipType { get; set; }
    public DateTime? JoinDate { get; set; }
}

/// <summary>
/// DTO for updating a visitor.
/// </summary>
public class UpdateVisitorDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MembershipType { get; set; }
    public DateTime? JoinDate { get; set; }
}

/// <summary>
/// DTO for visitor response.
/// </summary>
public class VisitorResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MembershipType { get; set; }
    public DateTime? JoinDate { get; set; }
}

/// <summary>
/// DTO for visitor statistics.
/// </summary>
public class VisitorStatisticsDto
{
    public int TotalVisitors { get; set; }
    public int TotalMembers { get; set; }
    public Dictionary<string, int> ByMembershipType { get; set; } = new();
}
