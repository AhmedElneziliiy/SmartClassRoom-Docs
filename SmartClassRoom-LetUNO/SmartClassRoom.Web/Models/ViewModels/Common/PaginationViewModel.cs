using Microsoft.AspNetCore.Mvc.Routing;

namespace SmartClassRoom.Web.Models.ViewModels.Common;

/// <summary>
/// View model for rendering pagination UI
/// </summary>
public class PaginationViewModel
{
    /// <summary>
    /// Current page number (1-indexed)
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// The starting item number for the current page (1-indexed)
    /// </summary>
    public int StartItemNumber { get; set; }

    /// <summary>
    /// The ending item number for the current page (1-indexed)
    /// </summary>
    public int EndItemNumber { get; set; }

    /// <summary>
    /// Route values to preserve when navigating (filters, sorting, etc.)
    /// </summary>
    public Dictionary<string, string?> RouteValues { get; set; } = new();

    /// <summary>
    /// Action name for pagination links
    /// </summary>
    public string ActionName { get; set; } = "Index";

    /// <summary>
    /// Controller name for pagination links
    /// </summary>
    public string? ControllerName { get; set; }

    /// <summary>
    /// Area name for pagination links
    /// </summary>
    public string? AreaName { get; set; }

    /// <summary>
    /// Gets the range of page numbers to display (sliding window)
    /// </summary>
    /// <param name="maxPagesToShow">Maximum number of page links to show (default: 5)</param>
    /// <returns>Array of page numbers to display</returns>
    public int[] GetPageRange(int maxPagesToShow = 5)
    {
        if (TotalPages <= maxPagesToShow)
        {
            // Show all pages if total is less than max
            return Enumerable.Range(1, TotalPages).ToArray();
        }

        // Calculate sliding window
        int halfWindow = maxPagesToShow / 2;
        int startPage = Math.Max(1, CurrentPage - halfWindow);
        int endPage = Math.Min(TotalPages, startPage + maxPagesToShow - 1);

        // Adjust start if we're near the end
        if (endPage - startPage < maxPagesToShow - 1)
        {
            startPage = Math.Max(1, endPage - maxPagesToShow + 1);
        }

        return Enumerable.Range(startPage, endPage - startPage + 1).ToArray();
    }

    /// <summary>
    /// Creates a PaginationViewModel from a PagedResult
    /// </summary>
    /// <typeparam name="T">Type of items in the paged result</typeparam>
    /// <param name="pagedResult">The paged result to create view model from</param>
    /// <param name="routeValues">Optional route values to preserve</param>
    /// <param name="actionName">Action name for links (default: "Index")</param>
    /// <param name="controllerName">Controller name for links (optional)</param>
    /// <param name="areaName">Area name for links (optional)</param>
    public static PaginationViewModel Create<T>(
        SmartClassRoom.Web.Models.Common.PagedResult<T> pagedResult,
        Dictionary<string, string?>? routeValues = null,
        string actionName = "Index",
        string? controllerName = null,
        string? areaName = null)
    {
        return new PaginationViewModel
        {
            CurrentPage = pagedResult.PageNumber,
            TotalPages = pagedResult.TotalPages,
            TotalItems = pagedResult.TotalCount,
            PageSize = pagedResult.PageSize,
            HasPrevious = pagedResult.HasPreviousPage,
            HasNext = pagedResult.HasNextPage,
            StartItemNumber = pagedResult.StartItemNumber,
            EndItemNumber = pagedResult.EndItemNumber,
            RouteValues = routeValues ?? new Dictionary<string, string?>(),
            ActionName = actionName,
            ControllerName = controllerName,
            AreaName = areaName
        };
    }
}
