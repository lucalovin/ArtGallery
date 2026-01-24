namespace ArtGallery.Application.DTOs.Common;

/// <summary>
/// Paged request parameters for list endpoints.
/// </summary>
public class PagedRequest
{
    private int _page = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
    }

    /// <summary>
    /// Gets or sets the field to sort by.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sort order (asc or desc).
    /// </summary>
    public string SortOrder { get; set; } = "asc";

    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets the number of items to skip.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Gets whether the sort order is descending.
    /// </summary>
    public bool IsDescending => SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);
}
