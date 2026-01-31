using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Dimension table for Artists in the Data Warehouse.
/// </summary>
public class DimArtist : BaseEntity
{
    /// <summary>
    /// Natural key from OLTP system.
    /// </summary>
    public int ArtistNk { get; set; }

    /// <summary>
    /// Artist's display name (for compatibility with DW schema).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Artist's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Artist's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Artist's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Artist's nationality.
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Year of birth.
    /// </summary>
    public int? BirthYear { get; set; }

    /// <summary>
    /// Year of death (null if still living).
    /// </summary>
    public int? DeathYear { get; set; }

    /// <summary>
    /// Art movement or style (e.g., Impressionism, Baroque).
    /// </summary>
    public string? ArtMovement { get; set; }

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
}
