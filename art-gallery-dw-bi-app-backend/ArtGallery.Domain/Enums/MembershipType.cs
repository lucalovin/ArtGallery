namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents visitor membership types.
/// </summary>
public enum MembershipType
{
    /// <summary>
    /// No membership - regular visitor.
    /// </summary>
    None = 0,

    /// <summary>
    /// Basic membership level.
    /// </summary>
    Basic = 1,

    /// <summary>
    /// Premium membership level with additional benefits.
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Patron membership level with exclusive access.
    /// </summary>
    Patron = 3,

    /// <summary>
    /// Student membership with discounted rates.
    /// </summary>
    Student = 4,

    /// <summary>
    /// Senior membership with discounted rates.
    /// </summary>
    Senior = 5,

    /// <summary>
    /// Family membership covering multiple family members.
    /// </summary>
    Family = 6
}
