using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class ScheduledSlotRepository : Repository<ScheduledSlot>, IScheduledSlotRepository
{
    public ScheduledSlotRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ScheduledSlot>> GetByTimetableAsync(int timetableId)
    {
        return await _dbSet
            .Include(ss => ss.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(ss => ss.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(ss => ss.Room)
            .Where(ss => ss.TimetableId == timetableId)
            .OrderBy(ss => ss.DayOfWeek)
            .ThenBy(ss => ss.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduledSlot>> GetByCourseOfferingAsync(int courseOfferingId)
    {
        return await _dbSet
            .Include(ss => ss.Timetable)
            .Include(ss => ss.Room)
            .Where(ss => ss.CourseOfferingId == courseOfferingId &&
                        ss.Timetable.Status == "Published" &&
                        ss.Timetable.IsActive)
            .OrderBy(ss => ss.DayOfWeek)
            .ThenBy(ss => ss.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduledSlot>> GetTeacherSlotsAsync(int teacherId, int termId)
    {
        return await _dbSet
            .Include(ss => ss.Timetable)
            .Include(ss => ss.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(ss => ss.Room)
            .Where(ss => ss.Timetable.TermId == termId && ss.CourseOffering.TeacherId == teacherId)
            .OrderBy(ss => ss.DayOfWeek)
            .ThenBy(ss => ss.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduledSlot>> GetSectionSlotsAsync(int sectionId, int termId)
    {
        return await _dbSet
            .Include(ss => ss.Timetable)
            .Include(ss => ss.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(ss => ss.Room)
            .Where(ss => ss.Timetable.TermId == termId && ss.CourseOffering.SectionId == sectionId)
            .OrderBy(ss => ss.DayOfWeek)
            .ThenBy(ss => ss.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScheduledSlot>> GetRoomSlotsAsync(int roomId, int termId)
    {
        return await _dbSet
            .Include(ss => ss.Timetable)
            .Include(ss => ss.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(ss => ss.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Where(ss => ss.Timetable.TermId == termId && ss.RoomId == roomId)
            .OrderBy(ss => ss.DayOfWeek)
            .ThenBy(ss => ss.StartTime)
            .ToListAsync();
    }

    public async Task<ScheduledSlot?> FindSlotAsync(string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int timetableId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ss => ss.TimetableId == timetableId &&
                                      ss.DayOfWeek == dayOfWeek &&
                                      ss.StartTime == startTime &&
                                      ss.EndTime == endTime);
    }

    public async Task<bool> HasTeacherConflictAsync(int teacherId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int termId, int? excludeSlotId = null)
    {
        return await _dbSet
            .Include(ss => ss.Timetable)
            .Include(ss => ss.CourseOffering)
            .AnyAsync(ss => ss.Timetable.TermId == termId &&
                           ss.CourseOffering.TeacherId == teacherId &&
                           ss.DayOfWeek == dayOfWeek &&
                           ss.StartTime < endTime &&
                           ss.EndTime > startTime &&
                           (!excludeSlotId.HasValue || ss.Id != excludeSlotId.Value));
    }

    public async Task<bool> HasSectionConflictAsync(int sectionId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int termId, int? excludeSlotId = null)
    {
        return await _dbSet
            .Include(ss => ss.Timetable)
            .Include(ss => ss.CourseOffering)
            .AnyAsync(ss => ss.Timetable.TermId == termId &&
                           ss.CourseOffering.SectionId == sectionId &&
                           ss.DayOfWeek == dayOfWeek &&
                           ss.StartTime < endTime &&
                           ss.EndTime > startTime &&
                           (!excludeSlotId.HasValue || ss.Id != excludeSlotId.Value));
    }

    public async Task<bool> HasRoomConflictAsync(int roomId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int termId, int? excludeSlotId = null)
    {
        return await _dbSet
            .Include(ss => ss.Timetable)
            .AnyAsync(ss => ss.Timetable.TermId == termId &&
                           ss.RoomId == roomId &&
                           ss.DayOfWeek == dayOfWeek &&
                           ss.StartTime < endTime &&
                           ss.EndTime > startTime &&
                           (!excludeSlotId.HasValue || ss.Id != excludeSlotId.Value));
    }
}
