﻿namespace ArtGallery.Domain.Entities;

/// <summary>
/// Represents a visitor to the gallery.
/// Maps to Oracle table: Visitor
/// </summary>
public class Visitor
{
    /// <summary>
    /// Gets or sets the visitor ID (maps to visitor_id in Oracle).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the visitor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visitor's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the visitor's phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the visitor's membership type.
    /// </summary>
    public string? MembershipType { get; set; }

    /// <summary>
    /// Gets or sets the date when the visitor joined.
    /// </summary>
    public DateTime? JoinDate { get; set; }
}
