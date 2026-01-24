using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Dimension table for Exhibitions in the Data Warehouse.
/// </summary>
public class DimExhibition : BaseEntity
{
    /// <summary>
    /// Natural key from OLTP system.
    /// </summary>
    public int ExhibitionNk { get; set; }

    /// <summary>
    /// Name of the exhibition.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the exhibition.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Start date of the exhibition.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the exhibition.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Duration in days.
    /// </summary>
    public int DurationDays { get; set; }

    /// <summary>
    /// Location/gallery room.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Type of exhibition (Permanent, Temporary, Traveling).
    /// </summary>
    public string? ExhibitionType { get; set; }

    /// <summary>
    /// Status (Planning, Active, Completed, Cancelled).
    /// </summary>
    public string Status { get; set; } = "Planning";

    /// <summary>
    /// Ticket price.
    /// </summary>
    public decimal? TicketPrice { get; set; }

    /// <summary>
    /// Budget allocated.
    /// </summary>
    public decimal? Budget { get; set; }

    /// <summary>
    /// SCD Type 2: Is this the current record?
    /// </summary>
    public bool IsCurrent { get; set; } = true;
}
