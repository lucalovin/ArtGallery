namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents the status of an exhibition.
/// </summary>
public enum ExhibitionStatus
{
    /// <summary>
    /// Exhibition is being planned.
    /// </summary>
    Planning = 0,

    /// <summary>
    /// Exhibition is upcoming and scheduled.
    /// </summary>
    Upcoming = 1,

    /// <summary>
    /// Exhibition is currently active and open to visitors.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Exhibition has ended.
    /// </summary>
    Past = 3,

    /// <summary>
    /// Exhibition has been cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Exhibition has been postponed.
    /// </summary>
    Postponed = 5
}
