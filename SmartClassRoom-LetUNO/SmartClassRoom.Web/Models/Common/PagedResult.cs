

namespace SmartClassRoom.Web.Models.Common;

/// <summary>
/// Generic wrapper for paginated data with metadata
/// </summary>
/// <typeparam name="T">The type of items in the page</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-indexed)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// The starting item number for the current page (1-indexed)
    /// </summary>
    public int StartItemNumber => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;

    /// <summary>
    /// The ending item number for the current page (1-indexed)
    /// </summary>
    public int EndItemNumber => Math.Min(PageNumber * PageSize, TotalCount);

    /// <summary>
    /// Creates a PagedResult from a repository tuple result
    /// </summary>
    /// <param name="items">The items in the current page</param>
    /// <param name="totalCount">Total number of items across all pages</param>
    /// <param name="pageNumber">Current page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Creates a PagedResult from a repository tuple result
    /// </summary>
    /// <param name="result">Tuple containing items and total count</param>
    /// <param name="pageNumber">Current page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    public static PagedResult<T> Create((IEnumerable<T> Items, int TotalCount) result, int pageNumber, int pageSize)
    {
        return Create(result.Items, result.TotalCount, pageNumber, pageSize);
    }
}
