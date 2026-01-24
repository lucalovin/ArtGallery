namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents the status of a restoration project.
/// </summary>
public enum RestorationStatus
{
    /// <summary>
    /// Restoration is scheduled.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Restoration is in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Restoration has been completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Restoration is on hold.
    /// </summary>
    OnHold = 3,

    /// <summary>
    /// Restoration has been cancelled.
    /// </summary>
    Cancelled = 4
}
