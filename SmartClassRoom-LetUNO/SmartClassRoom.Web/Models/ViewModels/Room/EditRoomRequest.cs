using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Room;

/// <summary>
/// Request model for editing an existing room
/// </summary>
public class EditRoomRequest
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Room Number is required")]
    [MaxLength(50, ErrorMessage = "Room Number cannot exceed 50 characters")]
    [Display(Name = "Room Number")]
    public string Number { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Room Name cannot exceed 100 characters")]
    [Display(Name = "Room Name")]
    public string? Name { get; set; }

    [MaxLength(50, ErrorMessage = "Room Type cannot exceed 50 characters")]
    [Display(Name = "Room Type")]
    public string? RoomType { get; set; }

    [Required(ErrorMessage = "Capacity is required")]
    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
    [Display(Name = "Capacity")]
    public int Capacity { get; set; }

    [MaxLength(100, ErrorMessage = "Building cannot exceed 100 characters")]
    [Display(Name = "Building")]
    public string? Building { get; set; }

    [MaxLength(10, ErrorMessage = "Floor cannot exceed 10 characters")]
    [Display(Name = "Floor")]
    public string? Floor { get; set; }

    [MaxLength(500, ErrorMessage = "Equipment cannot exceed 500 characters")]
    [Display(Name = "Equipment")]
    public string? Equipment { get; set; }

    [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    [Display(Name = "Status")]
    public string? Status { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Current DeviceId (read-only, set by first device)
    /// </summary>
    [Display(Name = "Device ID")]
    public string? DeviceId { get; set; }
}
