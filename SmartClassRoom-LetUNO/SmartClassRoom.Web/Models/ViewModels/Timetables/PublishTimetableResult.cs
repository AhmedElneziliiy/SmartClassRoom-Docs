namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class PublishTimetableResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SessionsCreated { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<ConflictInfo> RemainingConflicts { get; set; } = new();
}
