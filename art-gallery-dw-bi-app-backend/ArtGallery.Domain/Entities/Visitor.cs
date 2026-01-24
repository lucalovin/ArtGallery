using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a visitor to the gallery.
/// </summary>
public class Visitor : BaseEntity
{
    /// <summary>
    /// Gets or sets the visitor's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visitor's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visitor's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visitor's phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the visitor's membership type.
    /// </summary>
    public string MembershipType { get; set; } = "None";

    /// <summary>
    /// Gets or sets the membership expiry date.
    /// </summary>
    public DateTime? MembershipExpiry { get; set; }

    /// <summary>
    /// Gets or sets the total number of visits.
    /// </summary>
    public int TotalVisits { get; set; }

    /// <summary>
    /// Gets or sets the date of the last visit.
    /// </summary>
    public DateTime? LastVisit { get; set; }

    /// <summary>
    /// Gets or sets the visitor's address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the visitor's city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the visitor's country.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets any notes about the visitor.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets the full name of the visitor.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
