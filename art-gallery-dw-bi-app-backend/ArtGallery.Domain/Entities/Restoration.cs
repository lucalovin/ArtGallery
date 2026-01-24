using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a restoration project for an artwork.
/// </summary>
public class Restoration : BaseEntity
{
    /// <summary>
    /// Gets or sets the ID of the artwork being restored.
    /// </summary>
    public int ArtworkId { get; set; }

    /// <summary>
    /// Gets or sets the artwork being restored.
    /// </summary>
    public virtual Artwork Artwork { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of restoration (e.g., Cleaning, Conservation, Repair).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the restoration work.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the start date of the restoration.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the restoration.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the status of the restoration.
    /// </summary>
    public string Status { get; set; } = "Scheduled";

    /// <summary>
    /// Gets or sets the conservator responsible for the restoration.
    /// </summary>
    public string? Conservator { get; set; }

    /// <summary>
    /// Gets or sets the estimated cost of the restoration.
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Gets or sets the actual cost of the restoration.
    /// </summary>
    public decimal? ActualCost { get; set; }

    /// <summary>
    /// Gets or sets the condition of the artwork before restoration.
    /// </summary>
    public string? ConditionBefore { get; set; }

    /// <summary>
    /// Gets or sets the condition of the artwork after restoration.
    /// </summary>
    public string? ConditionAfter { get; set; }

    /// <summary>
    /// Gets or sets any additional notes about the restoration.
    /// </summary>
    public string? Notes { get; set; }
}
