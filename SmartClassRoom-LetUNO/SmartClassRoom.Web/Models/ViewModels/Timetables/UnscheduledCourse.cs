namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class UnscheduledCourse
{
    public int CourseOfferingId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int SessionsPerWeek { get; set; }
    public int ScheduledSessions { get; set; }
    public string Reason { get; set; } = string.Empty; // "No available time slots", "No suitable room"
}
