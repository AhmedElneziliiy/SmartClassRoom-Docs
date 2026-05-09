using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Session entity
/// </summary>
public class SessionRepository : Repository<Session>, ISessionRepository
{
    public SessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Session>> GetByCourseOfferingAsync(int courseOfferingId)
    {
        return await _dbSet
            .Where(s => s.CourseOfferingId == courseOfferingId)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.SessionDate >= startDate && s.SessionDate <= endDate)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetUpcomingSessionsAsync(int daysAhead = 7)
    {
        var today = DateTime.UtcNow.Date;
        var endDate = today.AddDays(daysAhead);

        return await _dbSet
            .Where(s => s.SessionDate >= today && s.SessionDate <= endDate)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByDateAsync(DateTime date)
    {
        return await _dbSet
            .Where(s => s.SessionDate.Date == date.Date)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByRoomAsync(int roomId)
    {
        return await _dbSet
            .Where(s => s.RoomId == roomId)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<Session?> GetSessionWithAttendanceAsync(int sessionId)
    {
        return await _dbSet
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .Include(s => s.AttendanceRecords)
                .ThenInclude(a => a.Student)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync()
    {
        return await _dbSet
            .Where(s => s.Status == "InProgress")
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByTeacherAsync(int teacherId)
    {
        return await _dbSet
            .Where(s => s.CourseOffering.TeacherId == teacherId)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.Room)
            .OrderByDescending(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByTeacherAndDateRangeAsync(
        int teacherId,
        DateTime startDate,
        DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.CourseOffering.TeacherId == teacherId &&
                       s.SessionDate >= startDate &&
                       s.SessionDate <= endDate)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.Room)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetTodaySessionsForTeacherAsync(int teacherId)
    {
        var today = DateTime.UtcNow.Date;

        return await _dbSet
            .Where(s => s.CourseOffering.TeacherId == teacherId &&
                       s.SessionDate.Date == today)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.Room)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetBySessionTypeAsync(string sessionType)
    {
        return await _dbSet
            .Where(s => s.SessionType == sessionType)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.Room)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<bool> IsRoomAvailableAsync(
        int roomId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan endTime,
        int? excludeSessionId = null)
    {
        var query = _dbSet
            .Where(s => s.RoomId == roomId &&
                       s.SessionDate.Date == date.Date &&
                       s.Status != "Cancelled" &&
                       ((s.StartTime < endTime && s.EndTime > startTime)));

        if (excludeSessionId.HasValue)
        {
            query = query.Where(s => s.Id != excludeSessionId.Value);
        }

        return !await query.AnyAsync();
    }
}
