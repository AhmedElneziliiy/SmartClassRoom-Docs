namespace SmartClassRoom.Web.Models.ViewModels.Reports;

public class ReportFilterDto
{
    public int? CourseOfferingId { get; set; }
    public int? TermId { get; set; }
    public int? StudentId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ReportType { get; set; } // "Attendance", "Grade", "Enrollment"
    public string? Format { get; set; } // "PDF", "Excel"
}
