namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class TimetableGenerationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? TimetableId { get; set; }
    public int TotalSlots { get; set; }
    public int ScheduledSlots { get; set; }
    public int UnscheduledSlots { get; set; }
    public List<ConflictInfo> Conflicts { get; set; } = new();
    public List<UnscheduledCourse> UnscheduledCourses { get; set; } = new();
    public string Status { get; set; } = "Draft"; // Draft or Published
}
