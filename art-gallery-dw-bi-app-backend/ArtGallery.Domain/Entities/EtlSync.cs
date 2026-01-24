namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an ETL synchronization record.
/// </summary>
public class EtlSync
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the sync operation.
    /// </summary>
    public DateTime SyncDate { get; set; }

    /// <summary>
    /// Gets or sets the status of the sync operation.
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets the number of records processed.
    /// </summary>
    public int RecordsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of records that failed.
    /// </summary>
    public int RecordsFailed { get; set; }

    /// <summary>
    /// Gets or sets the duration of the sync in milliseconds.
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// Gets or sets the source system for the sync.
    /// </summary>
    public string SourceSystem { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target system for the sync.
    /// </summary>
    public string TargetSystem { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of sync operation (e.g., Full, Incremental).
    /// </summary>
    public string SyncType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any error message if the sync failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets additional details about the sync.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
