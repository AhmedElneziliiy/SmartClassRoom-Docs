using SmartClassRoom.Web.Models.Entities.Attendance;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Attendance entity with domain-specific queries
/// </summary>
public interface IAttendanceRepository : IRepository<Attendance>
{
    /// <summary>
    /// Get attendance records by session
    /// </summary>
    Task<IEnumerable<Attendance>> GetBySessionAsync(int sessionId);

    /// <summary>
    /// Get attendance records by student
    /// </summary>
    Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId);

    /// <summary>
    /// Get attendance records by student for a specific course offering
    /// </summary>
    Task<IEnumerable<Attendance>> GetByStudentAndCourseOfferingAsync(
        int studentId,
        int courseOfferingId);

    /// <summary>
    /// Get attendance records by date range
    /// </summary>
    Task<IEnumerable<Attendance>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get attendance records by session and date range
    /// </summary>
    Task<IEnumerable<Attendance>> GetBySessionAndDateRangeAsync(
        int sessionId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get attendance statistics for a student
    /// </summary>
    Task<AttendanceStatistics> GetAttendanceStatisticsAsync(int studentId);

    /// <summary>
    /// Get attendance statistics for a student in a specific course offering
    /// </summary>
    Task<AttendanceStatistics> GetAttendanceStatisticsByCourseAsync(
        int studentId,
        int courseOfferingId);

    /// <summary>
    /// Get attendance rate for a session
    /// </summary>
    Task<SessionAttendanceRate> GetSessionAttendanceRateAsync(int sessionId);

    /// <summary>
    /// Get attendance records by verification method
    /// </summary>
    Task<IEnumerable<Attendance>> GetByVerificationMethodAsync(string verificationMethod);

    /// <summary>
    /// Get attendance records marked by a specific teacher
    /// </summary>
    Task<IEnumerable<Attendance>> GetMarkedByTeacherAsync(int teacherId);

    /// <summary>
    /// Check if student has attendance for session
    /// </summary>
    Task<bool> HasAttendanceForSessionAsync(int studentId, int sessionId);

    /// <summary>
    /// Get attendance record for a student in a specific session
    /// </summary>
    Task<Attendance?> GetByStudentAndSessionAsync(int studentId, int sessionId);

    /// <summary>
    /// Get count of checked-in students for a session
    /// </summary>
    Task<int> GetCheckedInCountAsync(int sessionId);
}

/// <summary>
/// DTO for attendance statistics
/// </summary>
public class AttendanceStatistics
{
    public int StudentId { get; set; }
    public int TotalSessions { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public decimal AttendancePercentage { get; set; }
}

/// <summary>
/// DTO for session attendance rate
/// </summary>
public class SessionAttendanceRate
{
    public int SessionId { get; set; }
    public int TotalEnrolled { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public decimal AttendanceRate { get; set; }
}
