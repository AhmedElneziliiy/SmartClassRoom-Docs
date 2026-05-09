namespace SmartClassRoom.Web.Models.DTOs.MobileAttendance;

/// <summary>
/// Response DTO for session management operations (start/end)
/// </summary>
public class SessionManagementResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public SessionManagementData? Data { get; set; }
}

/// <summary>
/// Data returned for session management operations
/// </summary>
public class SessionManagementData
{
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public int EnrolledCount { get; set; }
    public int CheckedInCount { get; set; }
}
