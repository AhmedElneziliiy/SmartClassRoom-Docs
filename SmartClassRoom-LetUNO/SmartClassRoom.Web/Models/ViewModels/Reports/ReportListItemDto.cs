namespace SmartClassRoom.Web.Models.ViewModels.Reports;

public class ReportListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int RecordCount { get; set; }
}
