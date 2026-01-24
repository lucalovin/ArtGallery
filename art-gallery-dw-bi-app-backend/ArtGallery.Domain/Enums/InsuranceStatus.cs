namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents the status of an insurance policy.
/// </summary>
public enum InsuranceStatus
{
    /// <summary>
    /// Insurance policy is pending activation.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Insurance policy is currently active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Insurance policy has expired.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Insurance policy has been cancelled.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Insurance claim is pending.
    /// </summary>
    ClaimPending = 4,

    /// <summary>
    /// Insurance claim has been processed.
    /// </summary>
    ClaimProcessed = 5
}
