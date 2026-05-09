namespace SmartClassRoom.Web.Models.ViewModels.Reports;

/// <summary>
/// DTO for Student Multi-Course Attendance Report
/// Shows attendance breakdown for a student across multiple selected courses
/// </summary>
public class StudentMultiCourseAttendanceDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public List<CourseAttendanceSummary> Courses { get; set; } = new();
}

/// <summary>
/// Summary of attendance for a single course
/// </summary>
public class CourseAttendanceSummary
{
    public int CourseOfferingId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public decimal AttendancePercentage { get; set; }
}
