using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Dimension table for Artworks in the Data Warehouse.
/// </summary>
public class DimArtwork : BaseEntity
{
    /// <summary>
    /// Natural key from OLTP system.
    /// </summary>
    public int ArtworkNk { get; set; }

    /// <summary>
    /// Title of the artwork.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to DimArtist.
    /// </summary>
    public int ArtistKey { get; set; }

    /// <summary>
    /// Year the artwork was created.
    /// </summary>
    public int? CreationYear { get; set; }

    /// <summary>
    /// Medium used (e.g., Oil on canvas).
    /// </summary>
    public string? Medium { get; set; }

    /// <summary>
    /// Dimensions as a string.
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Collection type (Permanent, Temporary, Loan).
    /// </summary>
    public string? CollectionType { get; set; }

    /// <summary>
    /// Current status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Estimated value.
    /// </summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>
    /// Date the artwork was acquired.
    /// </summary>
    public DateTime? AcquisitionDate { get; set; }

    /// <summary>
    /// SCD Type 2: Effective start date.
    /// </summary>
    public DateTime EffectiveStartDate { get; set; }

    /// <summary>
    /// SCD Type 2: Effective end date.
    /// </summary>
    public DateTime? EffectiveEndDate { get; set; }

    /// <summary>
    /// SCD Type 2: Is this the current record?
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    // Navigation property
    public virtual DimArtist? Artist { get; set; }
}
