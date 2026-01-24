using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Dimension table for Insurance in the Data Warehouse.
/// </summary>
public class DimInsurance : BaseEntity
{
    /// <summary>
    /// Natural key from OLTP system.
    /// </summary>
    public int InsuranceNk { get; set; }

    /// <summary>
    /// Policy number.
    /// </summary>
    public string PolicyNumber { get; set; } = string.Empty;

    /// <summary>
    /// Insurance provider.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Type of coverage.
    /// </summary>
    public string? CoverageType { get; set; }

    /// <summary>
    /// Coverage amount.
    /// </summary>
    public decimal? CoverageAmount { get; set; }

    /// <summary>
    /// Premium amount.
    /// </summary>
    public decimal? Premium { get; set; }

    /// <summary>
    /// Policy start date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Policy end date.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Policy status (Active, Expired, Cancelled).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// SCD Type 2: Is this the current record?
    /// </summary>
    public bool IsCurrent { get; set; } = true;
}
