namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents an exhibitor (external institution that can host exhibitions or receive loans).
/// Maps to Oracle table: Exhibitor
/// </summary>
public class Exhibitor
{
    /// <summary>
    /// Gets or sets the exhibitor ID (maps to exhibitor_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the exhibitor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the exhibitor.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the city of the exhibitor.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the contact information for the exhibitor.
    /// </summary>
    public string? ContactInfo { get; set; }

    /// <summary>
    /// Navigation property to the exhibitions organized by this exhibitor.
    /// </summary>
    public virtual ICollection<Exhibition> Exhibitions { get; set; } = new List<Exhibition>();

    /// <summary>
    /// Navigation property to the loans to this exhibitor.
    /// </summary>
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
