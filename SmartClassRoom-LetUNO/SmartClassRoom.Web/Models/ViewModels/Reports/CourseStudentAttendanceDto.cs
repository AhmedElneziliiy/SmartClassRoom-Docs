namespace SmartClassRoom.Web.Models.ViewModels.Reports;

/// <summary>
/// DTO for Course Student Attendance Report with session-by-session breakdown
/// Shows all students' attendance for a specific course offering
/// </summary>
public class CourseStudentAttendanceDto
{
    public int CourseOfferingId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<SessionDetailDto> Sessions { get; set; } = new();
    public List<StudentAttendanceDetailDto> Students { get; set; } = new();
}

/// <summary>
/// Details about a specific session
/// </summary>
public class SessionDetailDto
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty; // Scheduled, InProgress, Completed, Cancelled
}

/// <summary>
/// Detailed attendance information for a student including session-by-session breakdown
/// </summary>
public class StudentAttendanceDetailDto
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
    public int LateArrivals { get; set; }
    public TimeSpan? AverageLateMinutes { get; set; }
    public List<SessionAttendanceStatus> SessionAttendances { get; set; } = new();
}

/// <summary>
/// Attendance status for a specific session
/// </summary>
public class SessionAttendanceStatus
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public string Status { get; set; } = string.Empty; // Present, Absent, Late, Excused
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int? MinutesLate { get; set; }
}
