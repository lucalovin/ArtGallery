namespace ArtGallery.Application.DTOs.Common;

/// <summary>
/// Paginated response wrapper for list endpoints.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Gets or sets the list of items.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the total count of all items.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Creates a paginated response from items.
    /// </summary>
    public static PaginatedResponse<T> Create(List<T> items, int totalCount, int page, int pageSize)
    {
        return new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
