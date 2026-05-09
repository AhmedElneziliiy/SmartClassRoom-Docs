namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class ScheduledSlotDto
{
    public int Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string? RoomNumber { get; set; }
    public int? RoomCapacity { get; set; }
}
