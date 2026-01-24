namespace ArtGallery.Domain.Enums;

/// <summary>
/// Represents the status of an artwork in the gallery.
/// </summary>
public enum ArtworkStatus
{
    /// <summary>
    /// Artwork is available for display or loan.
    /// </summary>
    Available = 0,

    /// <summary>
    /// Artwork is currently on display in an exhibition.
    /// </summary>
    OnDisplay = 1,

    /// <summary>
    /// Artwork is currently on loan to another institution.
    /// </summary>
    OnLoan = 2,

    /// <summary>
    /// Artwork is in storage and not currently displayed.
    /// </summary>
    InStorage = 3,

    /// <summary>
    /// Artwork is undergoing restoration or conservation work.
    /// </summary>
    UnderRestoration = 4,

    /// <summary>
    /// Artwork is temporarily unavailable.
    /// </summary>
    Unavailable = 5,

    /// <summary>
    /// Artwork has been sold or transferred.
    /// </summary>
    Sold = 6,

    /// <summary>
    /// Artwork has been deaccessioned from the collection.
    /// </summary>
    Deaccessioned = 7
}
