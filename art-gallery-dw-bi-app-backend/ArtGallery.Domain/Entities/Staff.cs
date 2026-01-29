namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a staff member of the gallery.
/// Maps to Oracle table: Staff
/// </summary>
public class Staff
{
    /// <summary>
    /// Gets or sets the staff ID (maps to staff_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the staff member.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role of the staff member.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the staff member was hired.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Gets or sets the certification level of the staff member.
    /// </summary>
    public string? CertificationLevel { get; set; }

    /// <summary>
    /// Navigation property to restorations performed by this staff member.
    /// </summary>
    public virtual ICollection<Restoration> Restorations { get; set; } = new List<Restoration>();
}
