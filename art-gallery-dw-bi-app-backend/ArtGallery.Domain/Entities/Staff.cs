using ArtGallery.Domain.Common;

namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a staff member of the gallery.
/// </summary>
public class Staff : BaseEntity
{
    /// <summary>
    /// Gets or sets the staff member's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the staff member's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the staff member's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the staff member's phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the department where the staff member works.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the staff member's position/title.
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the staff member was hired.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Gets or sets the staff member's salary.
    /// </summary>
    public decimal? Salary { get; set; }

    /// <summary>
    /// Gets or sets the employment status.
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Gets or sets the URL of the staff member's profile image.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the staff member's biography.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gets the full name of the staff member.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
