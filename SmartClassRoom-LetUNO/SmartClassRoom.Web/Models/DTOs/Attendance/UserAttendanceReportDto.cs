namespace SmartClassRoom.Web.Models.DTOs.Attendance;

/// <summary>
/// Detailed attendance report for a student in a course
/// </summary>
public class UserAttendanceReportDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;

    // Summary Statistics
    public int TotalSessions { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public decimal AttendancePercentage { get; set; }

    // Detailed Records
    public List<AttendanceRecordDto> Records { get; set; } = new();
}

/// <summary>
/// Individual attendance record for a session
/// </summary>
public class AttendanceRecordDto
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty;  // Present, Absent, Late, Excused
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? Notes { get; set; }
}
