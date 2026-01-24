using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Dimension table for Visitors in the Data Warehouse.
/// </summary>
public class DimVisitor : BaseEntity
{
    /// <summary>
    /// Natural key from OLTP system.
    /// </summary>
    public int VisitorNk { get; set; }

    /// <summary>
    /// Visitor's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Visitor's email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Membership type (Regular, Premium, VIP).
    /// </summary>
    public string? MembershipType { get; set; }

    /// <summary>
    /// Age group (Child, Adult, Senior).
    /// </summary>
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Country of origin.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Date of first visit.
    /// </summary>
    public DateTime? FirstVisitDate { get; set; }

    /// <summary>
    /// SCD Type 2: Is this the current record?
    /// </summary>
    public bool IsCurrent { get; set; } = true;
}
