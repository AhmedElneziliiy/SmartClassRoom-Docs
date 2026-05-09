namespace SmartClassRoom.Web.Models.ViewModels.Reports;

/// <summary>
/// DTO for Teacher Attendance Report
/// Tracks teacher attendance for their scheduled sessions
/// </summary>
public class TeacherAttendanceDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalScheduledSessions { get; set; }
    public int SessionsAttended { get; set; }
    public int SessionsMissed { get; set; }
    public int LateArrivals { get; set; }
    public decimal AttendancePercentage { get; set; }
    public List<TeacherSessionDetailDto> Sessions { get; set; } = new();
}

/// <summary>
/// Details about a teacher's attendance for a specific session
/// </summary>
public class TeacherSessionDetailDto
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public TimeSpan ScheduledStartTime { get; set; }
    public TimeSpan ScheduledEndTime { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Present, Absent, Late
    public DateTime? ActualCheckInTime { get; set; }
    public DateTime? ActualCheckOutTime { get; set; }
    public int? MinutesLate { get; set; }
    public string RoomName { get; set; } = string.Empty;
}
