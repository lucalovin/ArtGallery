using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Dimension table for Locations in the Data Warehouse.
/// </summary>
public class DimLocation : BaseEntity
{
    /// <summary>
    /// Natural key from OLTP system.
    /// </summary>
    public int LocationNk { get; set; }

    /// <summary>
    /// Location name.
    /// </summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>
    /// Building name.
    /// </summary>
    public string? Building { get; set; }

    /// <summary>
    /// Floor number.
    /// </summary>
    public string? Floor { get; set; }

    /// <summary>
    /// Room/gallery name.
    /// </summary>
    public string? Room { get; set; }

    /// <summary>
    /// Location type (Gallery, Storage, Restoration Lab).
    /// </summary>
    public string? LocationType { get; set; }

    /// <summary>
    /// Capacity in number of artworks.
    /// </summary>
    public int? Capacity { get; set; }

    /// <summary>
    /// Square footage.
    /// </summary>
    public decimal? SquareFootage { get; set; }

    /// <summary>
    /// Is climate controlled.
    /// </summary>
    public bool IsClimateControlled { get; set; }

    /// <summary>
    /// Is public accessible.
    /// </summary>
    public bool IsPublicAccessible { get; set; }

    /// <summary>
    /// SCD Type 2: Is this the current record?
    /// </summary>
    public bool IsCurrent { get; set; } = true;
}
