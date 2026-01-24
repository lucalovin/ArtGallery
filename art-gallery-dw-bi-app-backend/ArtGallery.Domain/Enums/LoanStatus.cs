namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents the status of a loan.
/// </summary>
public enum LoanStatus
{
    /// <summary>
    /// Loan request is pending approval.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Loan has been approved.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Loan is currently active.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Loan has been returned.
    /// </summary>
    Returned = 3,

    /// <summary>
    /// Loan is overdue.
    /// </summary>
    Overdue = 4,

    /// <summary>
    /// Loan request has been rejected.
    /// </summary>
    Rejected = 5,

    /// <summary>
    /// Loan has been cancelled.
    /// </summary>
    Cancelled = 6
}
