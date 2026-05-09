using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.DTOs.MobileAttendance;

/// <summary>
/// Request DTO for student attendance check-in via mobile app
/// </summary>
public class CheckInRequest
{
    /// <summary>
    /// Mobile device unique identifier - must match user's stored UDID
    /// </summary>
    [Required(ErrorMessage = "UDID is required")]
    [MaxLength(100)]
    public string UDID { get; set; } = string.Empty;

    /// <summary>
    /// Session ID to check into
    /// </summary>
    [Required(ErrorMessage = "SessionId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid SessionId")]
    public int SessionId { get; set; }

    /// <summary>
    /// List of DeviceIds detected by mobile app (ESP device modulation numbers)
    /// At least one must match the session's room DeviceId
    /// </summary>
    [Required(ErrorMessage = "At least one DeviceId is required")]
    [MinLength(1, ErrorMessage = "At least one DeviceId must be provided")]
    public List<string> DeviceIds { get; set; } = new();
}
