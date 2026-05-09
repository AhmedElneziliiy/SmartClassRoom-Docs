namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Dashboard service interface for business logic
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    Task<DashboardStatistics> GetDashboardStatisticsAsync();

    /// <summary>
    /// Get statistics for a specific department
    /// </summary>
    Task<DepartmentDashboardStatistics> GetDepartmentStatisticsAsync(int departmentId);

    /// <summary>
    /// Get today's sessions summary
    /// </summary>
    Task<IEnumerable<TodaySessionSummary>> GetTodaySessionsAsync();

    /// <summary>
    /// Get recent attendance statistics
    /// </summary>
    Task<RecentAttendanceStatistics> GetRecentAttendanceStatisticsAsync(int days = 7);
}

/// <summary>
/// DTO for general dashboard statistics
/// </summary>
public class DashboardStatistics
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int ActiveTeachers { get; set; }
    public int TotalDepartments { get; set; }
    public int ActiveDepartments { get; set; }
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int TodaySessionsCount { get; set; }
    public int ActiveSessionsCount { get; set; }
}

/// <summary>
/// DTO for department-specific dashboard statistics
/// </summary>
public class DepartmentDashboardStatistics
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int CourseCount { get; set; }
    public int ActiveCourseOfferingsCount { get; set; }
    public decimal AverageAttendanceRate { get; set; }
}

/// <summary>
/// DTO for today's session summary
/// </summary>
public class TodaySessionSummary
{
    public int SessionId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? RoomName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int EnrolledStudents { get; set; }
    public int AttendedStudents { get; set; }
}

/// <summary>
/// DTO for recent attendance statistics
/// </summary>
public class RecentAttendanceStatistics
{
    public int TotalAttendanceRecords { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public decimal AverageAttendanceRate { get; set; }
}
