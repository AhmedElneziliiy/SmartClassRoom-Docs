namespace SmartClassRoom.Web.Models.ViewModels.Reports;

public class AttendanceReportDto
{
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int TotalSessions { get; set; }
    public List<AttendanceReportItemDto> StudentRecords { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

public class AttendanceReportItemDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public decimal AttendancePercentage { get; set; }
}
