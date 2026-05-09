using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Session entity with domain-specific queries
/// </summary>
public interface ISessionRepository : IRepository<Session>
{
    /// <summary>
    /// Get sessions by course offering
    /// </summary>
    Task<IEnumerable<Session>> GetByCourseOfferingAsync(int courseOfferingId);

    /// <summary>
    /// Get sessions by date range
    /// </summary>
    Task<IEnumerable<Session>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get upcoming sessions
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead (default 7)</param>
    Task<IEnumerable<Session>> GetUpcomingSessionsAsync(int daysAhead = 7);

    /// <summary>
    /// Get sessions for a specific date
    /// </summary>
    Task<IEnumerable<Session>> GetByDateAsync(DateTime date);

    /// <summary>
    /// Get sessions by room
    /// </summary>
    Task<IEnumerable<Session>> GetByRoomAsync(int roomId);

    /// <summary>
    /// Get sessions by status
    /// </summary>
    Task<IEnumerable<Session>> GetByStatusAsync(string status);

    /// <summary>
    /// Get session with attendance records
    /// </summary>
    Task<Session?> GetSessionWithAttendanceAsync(int sessionId);

    /// <summary>
    /// Get active (in-progress) sessions
    /// </summary>
    Task<IEnumerable<Session>> GetActiveSessionsAsync();

    /// <summary>
    /// Get sessions by teacher
    /// </summary>
    Task<IEnumerable<Session>> GetByTeacherAsync(int teacherId);

    /// <summary>
    /// Get sessions by teacher and date range
    /// </summary>
    Task<IEnumerable<Session>> GetByTeacherAndDateRangeAsync(
        int teacherId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Get today's sessions for a teacher
    /// </summary>
    Task<IEnumerable<Session>> GetTodaySessionsForTeacherAsync(int teacherId);

    /// <summary>
    /// Get sessions by session type
    /// </summary>
    Task<IEnumerable<Session>> GetBySessionTypeAsync(string sessionType);

    /// <summary>
    /// Check if room is available at a specific time
    /// </summary>
    Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        int? excludeSessionId = null);
}
