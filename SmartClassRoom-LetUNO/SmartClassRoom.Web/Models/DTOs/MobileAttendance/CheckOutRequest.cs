using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.DTOs.MobileAttendance;

/// <summary>
/// Request DTO for student attendance check-out via mobile app
/// </summary>
public class CheckOutRequest
{
    /// <summary>
    /// Mobile device unique identifier - must match user's stored UDID
    /// </summary>
    [Required(ErrorMessage = "UDID is required")]
    [MaxLength(100)]
    public string UDID { get; set; } = string.Empty;

    /// <summary>
    /// Session ID to check out from
    /// </summary>
    [Required(ErrorMessage = "SessionId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid SessionId")]
    public int SessionId { get; set; }

    /// <summary>
    /// Optional: List of DeviceIds for location verification
    /// </summary>
    public List<string>? DeviceIds { get; set; }
}
