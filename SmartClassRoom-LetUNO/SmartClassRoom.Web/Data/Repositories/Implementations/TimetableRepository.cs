using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class TimetableRepository : Repository<Timetable>, ITimetableRepository
{
    public TimetableRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Timetable?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Department)
            .Include(t => t.Level)
            .Include(t => t.Section)
            .Include(t => t.Term)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Timetable?> GetByIdWithSlotsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Department)
            .Include(t => t.Level)
            .Include(t => t.Section)
            .Include(t => t.Term)
            .Include(t => t.ScheduledSlots)
                .ThenInclude(ss => ss.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(t => t.ScheduledSlots)
                .ThenInclude(ss => ss.CourseOffering)
                    .ThenInclude(co => co.Teacher)
            .Include(t => t.ScheduledSlots)
                .ThenInclude(ss => ss.CourseOffering)
                    .ThenInclude(co => co.Section)
            .Include(t => t.ScheduledSlots)
                .ThenInclude(ss => ss.Room)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Timetable>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Include(t => t.Term)
            .Include(t => t.Level)
            .Include(t => t.Section)
            .Where(t => t.DepartmentId == departmentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Timetable>> GetByTermAsync(int termId)
    {
        return await _dbSet
            .Include(t => t.Department)
            .Include(t => t.Level)
            .Include(t => t.Section)
            .Where(t => t.TermId == termId)
            .OrderBy(t => t.Department.Name)
            .ThenBy(t => t.Level.Name)
            .ThenBy(t => t.Section != null ? t.Section.Name : "")
            .ToListAsync();
    }

    public async Task<Timetable?> GetBySectionAndTermAsync(int sectionId, int termId)
    {
        return await _dbSet
            .Include(t => t.ScheduledSlots)
            .FirstOrDefaultAsync(t => t.SectionId == sectionId && t.TermId == termId);
    }

    public async Task<IEnumerable<ScheduledSlot>> GetAllSlotsForTimetableAsync(int timetableId)
    {
        var timetable = await _dbSet
            .Include(t => t.ScheduledSlots)
                .ThenInclude(ss => ss.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(t => t.ScheduledSlots)
                .ThenInclude(ss => ss.CourseOffering)
                    .ThenInclude(co => co.Teacher)
            .Include(t => t.ScheduledSlots)
                .ThenInclude(ss => ss.Room)
            .FirstOrDefaultAsync(t => t.Id == timetableId);

        return timetable?.ScheduledSlots ?? Enumerable.Empty<ScheduledSlot>();
    }

    public async Task<bool> HasPublishedTimetableAsync(int sectionId, int termId)
    {
        return await _dbSet
            .AnyAsync(t => t.SectionId == sectionId &&
                          t.TermId == termId &&
                          t.Status == "Published");
    }

    public async Task<IEnumerable<Timetable>> GetDraftTimetablesAsync()
    {
        return await _dbSet
            .Include(t => t.Department)
            .Include(t => t.Level)
            .Include(t => t.Section)
            .Include(t => t.Term)
            .Where(t => t.Status == "Draft")
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Timetable>> GetPublishedTimetablesAsync(int termId)
    {
        return await _dbSet
            .Include(t => t.Department)
            .Include(t => t.Level)
            .Include(t => t.Section)
            .Where(t => t.TermId == termId && t.Status == "Published")
            .OrderBy(t => t.Department.Name)
            .ThenBy(t => t.Level.Name)
            .ThenBy(t => t.Section != null ? t.Section.Name : "")
            .ToListAsync();
    }
}
