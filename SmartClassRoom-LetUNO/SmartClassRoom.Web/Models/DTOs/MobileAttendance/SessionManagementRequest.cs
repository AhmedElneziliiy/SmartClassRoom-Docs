using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.DTOs.MobileAttendance;

/// <summary>
/// Request DTO for starting or ending a session (Teacher/Admin)
/// </summary>
public class SessionManagementRequest
{
    /// <summary>
    /// Session ID to start or end
    /// </summary>
    [Required(ErrorMessage = "SessionId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid SessionId")]
    public int SessionId { get; set; }

    /// <summary>
    /// Optional: UDID for mobile verification
    /// </summary>
    public string? UDID { get; set; }

    /// <summary>
    /// List of nearby ESP32 device IDs (modulation strings) detected by the mobile app
    /// Required for teacher attendance verification
    /// </summary>
    public List<string> DeviceIds { get; set; } = new();
}
