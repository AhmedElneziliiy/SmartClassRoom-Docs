namespace SmartClassRoom.Web.Models.DTOs.MobileAttendance;

/// <summary>
/// Response DTO for check-out operation
/// </summary>
public class CheckOutResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public CheckOutData? Data { get; set; }
}

/// <summary>
/// Data returned on successful check-out
/// </summary>
public class CheckOutData
{
    public int AttendanceId { get; set; }
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime CheckOutTime { get; set; }
    public string Duration { get; set; } = string.Empty; // "1h 30m"
    public string Status { get; set; } = string.Empty;
}
