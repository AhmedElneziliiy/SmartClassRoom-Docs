using System.Text.Json.Serialization;

namespace SmartClassRoom.Web.Models.DTOs.Timetable;

/// <summary>
/// DTO for weekly schedule view - supports both Students and Teachers
/// </summary>
public class UserScheduleDto
{
    // Generic user properties
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty; // "Student" or "Teacher"

    // Backward compatibility - only include for Students (null for Teachers will be ignored)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StudentId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StudentName { get; set; }

    public int? TermId { get; set; }
    public string? TermName { get; set; }
    public List<ScheduleItemDto> Schedule { get; set; } = new();
}

/// <summary>
/// DTO for today's schedule view - supports both Students and Teachers
/// </summary>
public class TodayScheduleDto
{
    // Generic user properties
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty; // "Student" or "Teacher"

    // Backward compatibility - only include for Students (null for Teachers will be ignored)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? StudentId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StudentName { get; set; }

    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public List<TodayScheduleItemDto> Classes { get; set; } = new();
}

/// <summary>
/// Individual class item for today's schedule
/// </summary>
public class TodayScheduleItemDto
{
    public int? SessionId { get; set; }
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? RoomName { get; set; }
    public string? RoomBuilding { get; set; }
    public string? SessionStatus { get; set; }  // Scheduled, InProgress, Completed, Cancelled
    public string? AttendanceStatus { get; set; }  // Student's attendance status if session exists
    public string? TeacherAttendanceStatus { get; set; }  // Teacher's attendance status (for teacher schedule view)
    public int? EnrolledCount { get; set; }  // For teachers: number of enrolled students
    public int? CheckedInCount { get; set; }  // For teachers: number of checked-in students
}

/// <summary>
/// Individual schedule item for weekly timetable
/// </summary>
public class ScheduleItemDto
{
    public int? SessionId { get; set; }
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? RoomName { get; set; }
    public string? RoomBuilding { get; set; }
    public int? EnrolledCount { get; set; }  // For teachers: number of enrolled students
}
