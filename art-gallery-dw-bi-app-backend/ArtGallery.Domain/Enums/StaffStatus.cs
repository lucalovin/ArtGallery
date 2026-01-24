namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents the employment status of a staff member.
/// </summary>
public enum StaffStatus
{
    /// <summary>
    /// Staff member is currently active.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Staff member is on leave.
    /// </summary>
    OnLeave = 1,

    /// <summary>
    /// Staff member is inactive.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Staff member has been terminated.
    /// </summary>
    Terminated = 3,

    /// <summary>
    /// Staff member has retired.
    /// </summary>
    Retired = 4
}
