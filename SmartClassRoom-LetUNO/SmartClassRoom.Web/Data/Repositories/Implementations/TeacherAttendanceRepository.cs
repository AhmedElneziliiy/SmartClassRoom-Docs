using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Attendance;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for TeacherAttendance entity
/// </summary>
public class TeacherAttendanceRepository : Repository<TeacherAttendance>, ITeacherAttendanceRepository
{
    public TeacherAttendanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TeacherAttendance?> GetByTeacherAndSessionAsync(int teacherId, int sessionId)
    {
        return await _dbSet
            .Include(ta => ta.Session)
                .ThenInclude(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(ta => ta.Session)
                .ThenInclude(s => s.Room)
            .Include(ta => ta.Teacher)
            .FirstOrDefaultAsync(ta => ta.TeacherId == teacherId && ta.SessionId == sessionId);
    }

    public async Task<IEnumerable<TeacherAttendance>> GetByTeacherAsync(int teacherId)
    {
        return await _dbSet
            .Include(ta => ta.Session)
                .ThenInclude(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(ta => ta.Session)
                .ThenInclude(s => s.Room)
            .Where(ta => ta.TeacherId == teacherId)
            .OrderByDescending(ta => ta.CheckInTime)
            .ToListAsync();
    }

    public async Task<TeacherAttendance?> GetBySessionAsync(int sessionId)
    {
        return await _dbSet
            .Include(ta => ta.Session)
                .ThenInclude(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(ta => ta.Teacher)
            .FirstOrDefaultAsync(ta => ta.SessionId == sessionId);
    }

    public async Task<IEnumerable<TeacherAttendance>> GetByTeacherAndDateRangeAsync(int teacherId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(ta => ta.Session)
                .ThenInclude(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(ta => ta.Session)
                .ThenInclude(s => s.Room)
            .Where(ta => ta.TeacherId == teacherId &&
                        ta.CheckInTime >= startDate &&
                        ta.CheckInTime <= endDate)
            .OrderByDescending(ta => ta.CheckInTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<TeacherAttendance>> GetByTeacherAndCourseOfferingAsync(int teacherId, int courseOfferingId)
    {
        return await _dbSet
            .Include(ta => ta.Session)
                .ThenInclude(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(ta => ta.Session)
                .ThenInclude(s => s.Room)
            .Where(ta => ta.TeacherId == teacherId &&
                        ta.Session.CourseOfferingId == courseOfferingId)
            .OrderByDescending(ta => ta.CheckInTime)
            .ToListAsync();
    }

    public async Task<bool> HasCheckedInAsync(int teacherId, int sessionId)
    {
        return await _dbSet
            .AnyAsync(ta => ta.TeacherId == teacherId && ta.SessionId == sessionId);
    }
}
