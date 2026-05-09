namespace SmartClassRoom.Web.Models.DTOs.MobileAttendance;

/// <summary>
/// DTO for listing active sessions available for check-in or management
/// </summary>
public class ActiveSessionDto
{
    public int SessionId { get; set; }
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? RoomName { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomDeviceId { get; set; }
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether the session has been started by teacher
    /// </summary>
    public bool IsStarted { get; set; }

    /// <summary>
    /// Whether the session can be checked into (InProgress status)
    /// </summary>
    public bool CanCheckIn { get; set; }

    /// <summary>
    /// Whether the user has already checked in
    /// </summary>
    public bool HasCheckedIn { get; set; }

    /// <summary>
    /// Whether the user has already checked out
    /// </summary>
    public bool HasCheckedOut { get; set; }

    /// <summary>
    /// If already checked in, the attendance status ("Present" or "Late")
    /// </summary>
    public string? AttendanceStatus { get; set; }

    /// <summary>
    /// Check-in time if already checked in
    /// </summary>
    public DateTime? CheckInTime { get; set; }

    /// <summary>
    /// Check-out time if already checked out
    /// </summary>
    public DateTime? CheckOutTime { get; set; }

    /// <summary>
    /// Number of enrolled students (for teacher view)
    /// </summary>
    public int EnrolledCount { get; set; }

    /// <summary>
    /// Number of checked-in students (for teacher view)
    /// </summary>
    public int CheckedInCount { get; set; }
}
