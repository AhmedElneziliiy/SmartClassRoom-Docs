namespace SmartClassRoom.Web.Models.DTOs.MobileAttendance;

/// <summary>
/// Response DTO for check-in operation
/// </summary>
public class CheckInResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public CheckInData? Data { get; set; }
}

/// <summary>
/// Data returned on successful check-in
/// </summary>
public class CheckInData
{
    public int AttendanceId { get; set; }
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public string Status { get; set; } = string.Empty; // "Present" or "Late"
    public bool IsLate { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public TimeSpan SessionStartTime { get; set; }
    public TimeSpan SessionEndTime { get; set; }
}
