namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class ConflictInfo
{
    public string ConflictType { get; set; } = string.Empty; // "Teacher", "Section", "Room"
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Resource { get; set; } = string.Empty; // Teacher name, Section name, or Room number
    public List<string> AffectedCourses { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}
