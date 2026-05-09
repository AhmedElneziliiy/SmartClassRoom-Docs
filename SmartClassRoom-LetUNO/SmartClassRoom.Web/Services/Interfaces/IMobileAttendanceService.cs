using SmartClassRoom.Web.Models.DTOs.MobileAttendance;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for mobile attendance operations (check-in, check-out, session management)
/// </summary>
public interface IMobileAttendanceService
{
    /// <summary>
    /// Process student check-in for a session
    /// Validates UDID, device proximity, enrollment, and session status
    /// </summary>
    /// <param name="userId">The authenticated user's ID</param>
    /// <param name="request">Check-in request with UDID, SessionId, and DeviceIds</param>
    /// <returns>Check-in response with success status and attendance data</returns>
    Task<CheckInResponse> CheckInAsync(int userId, CheckInRequest request);

    /// <summary>
    /// Process student check-out for a session
    /// Validates UDID and existing attendance record
    /// </summary>
    /// <param name="userId">The authenticated user's ID</param>
    /// <param name="request">Check-out request with UDID and SessionId</param>
    /// <returns>Check-out response with success status and duration</returns>
    Task<CheckOutResponse> CheckOutAsync(int userId, CheckOutRequest request);

    /// <summary>
    /// Start a session (Teacher or Admin only)
    /// Changes session status from Scheduled to InProgress
    /// Also records teacher check-in with device proximity verification
    /// </summary>
    /// <param name="userId">The authenticated user's ID (must be teacher or admin)</param>
    /// <param name="request">Session management request with SessionId, DeviceIds for proximity</param>
    /// <returns>Session management response with updated session data</returns>
    Task<SessionManagementResponse> StartSessionAsync(int userId, SessionManagementRequest request);

    /// <summary>
    /// End a session (Teacher or Admin only)
    /// Changes session status from InProgress to Completed
    /// Also records teacher check-out with device proximity verification
    /// </summary>
    /// <param name="userId">The authenticated user's ID (must be teacher or admin)</param>
    /// <param name="request">Session management request with SessionId, DeviceIds for proximity</param>
    /// <returns>Session management response with updated session data</returns>
    Task<SessionManagementResponse> EndSessionAsync(int userId, SessionManagementRequest request);

    /// <summary>
    /// Get active/upcoming sessions available for a student to check into
    /// Based on their enrollments
    /// </summary>
    /// <param name="studentId">The student's ID</param>
    /// <returns>List of active sessions with check-in status</returns>
    Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsForStudentAsync(int studentId);

    /// <summary>
    /// Get sessions that a teacher can manage (start/end)
    /// Based on their course offerings
    /// </summary>
    /// <param name="teacherId">The teacher's ID</param>
    /// <returns>List of sessions the teacher can manage</returns>
    Task<IEnumerable<ActiveSessionDto>> GetManageableSessionsForTeacherAsync(int teacherId);

    /// <summary>
    /// Get the current attendance status for a student in a session
    /// </summary>
    /// <param name="studentId">The student's ID</param>
    /// <param name="sessionId">The session ID</param>
    /// <returns>Attendance status details or null if not checked in</returns>
    Task<AttendanceStatusDto?> GetAttendanceStatusAsync(int studentId, int sessionId);

    /// <summary>
    /// Get the current attendance status for a teacher in a session
    /// </summary>
    /// <param name="teacherId">The teacher's ID</param>
    /// <param name="sessionId">The session ID</param>
    /// <returns>Teacher attendance status details or null if not checked in</returns>
    Task<TeacherAttendanceStatusDto?> GetTeacherAttendanceStatusAsync(int teacherId, int sessionId);

    /// <summary>
    /// Force checkout teacher and all students when session grace period ends
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>Number of attendances force-checked out</returns>
    Task<int> ForceCheckoutSessionAsync(int sessionId);
}

/// <summary>
/// DTO for student attendance status response
/// </summary>
public class AttendanceStatusDto
{
    public int AttendanceId { get; set; }
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public bool IsCheckedOut { get; set; }
    public string? Duration { get; set; }
}

/// <summary>
/// DTO for teacher attendance status response
/// </summary>
public class TeacherAttendanceStatusDto
{
    public int AttendanceId { get; set; }
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public bool IsCheckedOut { get; set; }
    public string? Duration { get; set; }
    public int EnrolledCount { get; set; }
    public int CheckedInCount { get; set; }
}
