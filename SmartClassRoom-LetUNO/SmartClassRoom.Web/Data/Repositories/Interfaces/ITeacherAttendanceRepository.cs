using SmartClassRoom.Web.Models.Entities.Attendance;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for TeacherAttendance entity
/// </summary>
public interface ITeacherAttendanceRepository : IRepository<TeacherAttendance>
{
    /// <summary>
    /// Get teacher attendance record for a specific session
    /// </summary>
    Task<TeacherAttendance?> GetByTeacherAndSessionAsync(int teacherId, int sessionId);

    /// <summary>
    /// Get all attendance records for a teacher
    /// </summary>
    Task<IEnumerable<TeacherAttendance>> GetByTeacherAsync(int teacherId);

    /// <summary>
    /// Get all attendance records for a session
    /// </summary>
    Task<TeacherAttendance?> GetBySessionAsync(int sessionId);

    /// <summary>
    /// Get teacher attendance records for a date range
    /// </summary>
    Task<IEnumerable<TeacherAttendance>> GetByTeacherAndDateRangeAsync(int teacherId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get teacher attendance records for a course offering
    /// </summary>
    Task<IEnumerable<TeacherAttendance>> GetByTeacherAndCourseOfferingAsync(int teacherId, int courseOfferingId);

    /// <summary>
    /// Check if teacher has checked in to a session
    /// </summary>
    Task<bool> HasCheckedInAsync(int teacherId, int sessionId);
}
