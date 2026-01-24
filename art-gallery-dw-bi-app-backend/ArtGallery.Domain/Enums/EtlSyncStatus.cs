namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents the status of an ETL synchronization operation.
/// </summary>
public enum EtlSyncStatus
{
    /// <summary>
    /// Sync is pending execution.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Sync is currently running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Sync completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Sync completed with warnings.
    /// </summary>
    CompletedWithWarnings = 3,

    /// <summary>
    /// Sync failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Sync was cancelled.
    /// </summary>
    Cancelled = 5
}
