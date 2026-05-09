using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartClassRoom.Web.Utilities.TagHelpers;

/// <summary>
/// Tag helper for creating sortable column headers in tables
/// Usage: <th asp-sortable-column="Name" asp-column-display="User Name"></th>
/// </summary>
[HtmlTargetElement("th", Attributes = "asp-sortable-column")]
public class SortableColumnHeaderTagHelper : TagHelper
{
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    /// <summary>
    /// The column name to sort by (e.g., "Name", "Email", "UserType")
    /// </summary>
    [HtmlAttributeName("asp-sortable-column")]
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// The display text for the column header (optional, defaults to ColumnName)
    /// </summary>
    [HtmlAttributeName("asp-column-display")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// The action name for the sort link (optional, defaults to current action)
    /// </summary>
    [HtmlAttributeName("asp-action")]
    public string? ActionName { get; set; }

    /// <summary>
    /// The controller name for the sort link (optional, defaults to current controller)
    /// </summary>
    [HtmlAttributeName("asp-controller")]
    public string? ControllerName { get; set; }

    /// <summary>
    /// The area name for the sort link (optional, defaults to current area)
    /// </summary>
    [HtmlAttributeName("asp-area")]
    public string? AreaName { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Get current sort values from ViewBag
        var currentSort = ViewContext.ViewBag.CurrentSort as string;
        var sortAscending = ViewContext.ViewBag.SortAscending as bool? ?? true;

        // Determine if this column is currently sorted
        var isCurrentSort = string.Equals(currentSort, ColumnName, StringComparison.OrdinalIgnoreCase);

        // Determine the new sort direction (toggle if same column, otherwise ascending)
        var newAscending = isCurrentSort ? !sortAscending : true;

        // Build the query string parameters
        var routeValues = new Dictionary<string, string>();

        // Add existing query parameters (filters, etc.)
        foreach (var key in ViewContext.HttpContext.Request.Query.Keys)
        {
            if (key != "sortBy" && key != "ascending" && key != "page")
            {
                routeValues[key] = ViewContext.HttpContext.Request.Query[key].ToString();
            }
        }

        // Add sort parameters
        routeValues["sortBy"] = ColumnName;
        routeValues["ascending"] = newAscending.ToString();
        routeValues["page"] = "1"; // Reset to first page when sorting

        // Build the URL
        var actionName = ActionName ?? ViewContext.RouteData.Values["action"]?.ToString() ?? "Index";
        var controllerName = ControllerName ?? ViewContext.RouteData.Values["controller"]?.ToString();
        var areaName = AreaName ?? ViewContext.RouteData.Values["area"]?.ToString();

        var queryString = string.Join("&", routeValues.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var baseUrl = $"/{areaName}/{controllerName}/{actionName}";
        var url = $"{baseUrl}?{queryString}";

        // Create the link content
        var displayText = DisplayName ?? ColumnName;
        var sortIcon = "";

        if (isCurrentSort)
        {
            sortIcon = sortAscending
                ? "<i class=\"bi bi-caret-up-fill sort-icon\"></i>"
                : "<i class=\"bi bi-caret-down-fill sort-icon\"></i>";
        }
        else
        {
            sortIcon = "<i class=\"bi bi-chevron-expand sort-icon-neutral\"></i>";
        }

        // Add CSS class for sortable column
        var existingClass = output.Attributes.FirstOrDefault(a => a.Name == "class")?.Value?.ToString() ?? "";
        var newClass = $"{existingClass} sortable-column".Trim();
        if (isCurrentSort)
        {
            newClass += " sorted";
        }

        output.Attributes.SetAttribute("class", newClass);

        // Set the content as a clickable link
        output.Content.SetHtmlContent($"<a href=\"{url}\" class=\"sort-link\">{displayText} {sortIcon}</a>");
    }
}
