namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class TimeSlotDto
{
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public override string ToString()
    {
        return $"{DayOfWeek} {StartTime:hh\\:mm}-{EndTime:hh\\:mm}";
    }
}
