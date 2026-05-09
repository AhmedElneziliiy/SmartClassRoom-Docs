namespace SmartClassRoom.Web.Models.DTOs.Attendance;

/// <summary>
/// DTO for listing courses with attendance summary
/// </summary>
public class CourseAttendanceListDto
{
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int AttendedSessions { get; set; }
    public decimal AttendancePercentage { get; set; }
}
