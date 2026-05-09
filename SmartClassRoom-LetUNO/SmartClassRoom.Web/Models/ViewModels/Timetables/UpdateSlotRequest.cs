using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class UpdateSlotRequest
{
    [Required]
    public int SlotId { get; set; }

    [Required]
    [MaxLength(20)]
    public string DayOfWeek { get; set; } = string.Empty;

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public int? RoomId { get; set; }
}
