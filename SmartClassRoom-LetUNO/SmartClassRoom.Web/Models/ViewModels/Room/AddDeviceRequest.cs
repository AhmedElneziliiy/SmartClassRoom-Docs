using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Room;

/// <summary>
/// Request model for adding an ESP device to a room
/// </summary>
public class AddDeviceRequest
{
    [Required]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Device ID (Modulation Number) is required")]
    [MaxLength(100, ErrorMessage = "Device ID cannot exceed 100 characters")]
    [Display(Name = "Device ID (Modulation Number)")]
    public string DeviceId { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "MAC Address cannot exceed 50 characters")]
    [Display(Name = "MAC Address")]
    public string? MacAddress { get; set; }

    [MaxLength(50, ErrorMessage = "IP Address cannot exceed 50 characters")]
    [Display(Name = "IP Address")]
    public string? IPAddress { get; set; }
}
