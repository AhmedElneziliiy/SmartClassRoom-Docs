using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class SwapSlotsRequest
{
    [Required]
    public int Slot1Id { get; set; }

    [Required]
    public int Slot2Id { get; set; }

    [Required]
    public int TimetableId { get; set; }
}
