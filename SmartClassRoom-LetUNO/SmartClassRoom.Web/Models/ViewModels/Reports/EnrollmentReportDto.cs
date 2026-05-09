namespace SmartClassRoom.Web.Models.ViewModels.Reports;

public class EnrollmentReportDto
{
    public int? TermId { get; set; }
    public string? TermName { get; set; }
    public int TotalOfferings { get; set; }
    public int TotalEnrollments { get; set; }
    public List<EnrollmentReportItemDto> Offerings { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

public class EnrollmentReportItemDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public int EnrolledCount { get; set; }
    public int ActiveCount { get; set; }
    public int DroppedCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal EnrollmentRate { get; set; }
}
