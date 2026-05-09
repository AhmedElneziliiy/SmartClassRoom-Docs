using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Attendance;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Attendance entity
/// </summary>
public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Attendance>> GetBySessionAsync(int sessionId)
    {
        return await _dbSet
            .Where(a => a.SessionId == sessionId)
            .Include(a => a.Student)
            .Include(a => a.Session)
            .Include(a => a.MarkedByTeacher)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId)
    {
        return await _dbSet
            .Where(a => a.StudentId == studentId)
            .Include(a => a.Session)
                .ThenInclude(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetByStudentAndCourseOfferingAsync(
        int studentId,
        int courseOfferingId)
    {
        return await _dbSet
            .Where(a => a.StudentId == studentId &&
                       a.Session.CourseOfferingId == courseOfferingId)
            .Include(a => a.Session)
            .OrderBy(a => a.Session.SessionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate)
    {
        return await _dbSet
            .Where(a => a.CheckInTime >= startDate && a.CheckInTime <= endDate)
            .Include(a => a.Student)
            .Include(a => a.Session)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetBySessionAndDateRangeAsync(
        int sessionId,
        DateTime startDate,
        DateTime endDate)
    {
        return await _dbSet
            .Where(a => a.SessionId == sessionId &&
                       a.CheckInTime >= startDate &&
                       a.CheckInTime <= endDate)
            .Include(a => a.Student)
            .OrderBy(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<AttendanceStatistics> GetAttendanceStatisticsAsync(int studentId)
    {
        var attendanceRecords = await _dbSet
            .Where(a => a.StudentId == studentId)
            .ToListAsync();

        var totalSessions = attendanceRecords.Count;
        var presentCount = attendanceRecords.Count(a => a.Status == "Present");
        var absentCount = attendanceRecords.Count(a => a.Status == "Absent");
        var lateCount = attendanceRecords.Count(a => a.Status == "Late");
        var excusedCount = attendanceRecords.Count(a => a.Status == "Excused");

        var attendancePercentage = totalSessions > 0
            ? (decimal)presentCount / totalSessions * 100
            : 0;

        return new AttendanceStatistics
        {
            StudentId = studentId,
            TotalSessions = totalSessions,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            LateCount = lateCount,
            ExcusedCount = excusedCount,
            AttendancePercentage = Math.Round(attendancePercentage, 2)
        };
    }

    public async Task<AttendanceStatistics> GetAttendanceStatisticsByCourseAsync(
        int studentId,
        int courseOfferingId)
    {
        var attendanceRecords = await _dbSet
            .Where(a => a.StudentId == studentId &&
                       a.Session.CourseOfferingId == courseOfferingId)
            .ToListAsync();

        var totalSessions = attendanceRecords.Count;
        var presentCount = attendanceRecords.Count(a => a.Status == "Present");
        var absentCount = attendanceRecords.Count(a => a.Status == "Absent");
        var lateCount = attendanceRecords.Count(a => a.Status == "Late");
        var excusedCount = attendanceRecords.Count(a => a.Status == "Excused");

        var attendancePercentage = totalSessions > 0
            ? (decimal)presentCount / totalSessions * 100
            : 0;

        return new AttendanceStatistics
        {
            StudentId = studentId,
            TotalSessions = totalSessions,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            LateCount = lateCount,
            ExcusedCount = excusedCount,
            AttendancePercentage = Math.Round(attendancePercentage, 2)
        };
    }

    public async Task<SessionAttendanceRate> GetSessionAttendanceRateAsync(int sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.CourseOffering)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            return new SessionAttendanceRate();
        }

        var totalEnrolled = await _context.StudentEnrollments
            .Where(e => e.CourseOfferingId == session.CourseOfferingId)
            .CountAsync();

        var attendanceRecords = await _dbSet
            .Where(a => a.SessionId == sessionId)
            .ToListAsync();

        var presentCount = attendanceRecords.Count(a => a.Status == "Present");
        var absentCount = totalEnrolled - presentCount;

        var attendanceRate = totalEnrolled > 0
            ? (decimal)presentCount / totalEnrolled * 100
            : 0;

        return new SessionAttendanceRate
        {
            SessionId = sessionId,
            TotalEnrolled = totalEnrolled,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            AttendanceRate = Math.Round(attendanceRate, 2)
        };
    }

    public async Task<IEnumerable<Attendance>> GetByVerificationMethodAsync(string verificationMethod)
    {
        return await _dbSet
            .Where(a => a.VerificationMethod == verificationMethod)
            .Include(a => a.Student)
            .Include(a => a.Session)
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetMarkedByTeacherAsync(int teacherId)
    {
        return await _dbSet
            .Where(a => a.MarkedBy == teacherId)
            .Include(a => a.Student)
            .Include(a => a.Session)
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<bool> HasAttendanceForSessionAsync(int studentId, int sessionId)
    {
        return await _dbSet
            .AnyAsync(a => a.StudentId == studentId && a.SessionId == sessionId);
    }

    public async Task<Attendance?> GetByStudentAndSessionAsync(int studentId, int sessionId)
    {
        return await _dbSet
            .Include(a => a.Session)
                .ThenInclude(s => s.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(a => a.Session)
                .ThenInclude(s => s.Room)
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.SessionId == sessionId);
    }

    public async Task<int> GetCheckedInCountAsync(int sessionId)
    {
        return await _dbSet
            .CountAsync(a => a.SessionId == sessionId && a.CheckInTime != default);
    }
}
