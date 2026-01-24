using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Dimension table for Staff in the Data Warehouse.
/// </summary>
public class DimStaff : BaseEntity
{
    /// <summary>
    /// Natural key from OLTP system.
    /// </summary>
    public int StaffNk { get; set; }

    /// <summary>
    /// Staff member's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Department.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Employment status (Active, Inactive, OnLeave).
    /// </summary>
    public string? EmploymentStatus { get; set; }

    /// <summary>
    /// Hire date.
    /// </summary>
    public DateTime? HireDate { get; set; }

    /// <summary>
    /// SCD Type 2: Is this the current record?
    /// </summary>
    public bool IsCurrent { get; set; } = true;
}
