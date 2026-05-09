using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class CourseOfferingRepository : Repository<CourseOffering>, ICourseOfferingRepository
{
    public CourseOfferingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CourseOffering>> GetByTermAsync(int termId)
    {
        return await _dbSet
            .Include(co => co.Course)
            .Include(co => co.Term)
            .Include(co => co.Teacher)
            .Include(co => co.Section)
            .Where(co => co.TermId == termId)
            .OrderBy(co => co.Course.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseOffering>> GetByCourseAsync(int courseId)
    {
        return await _dbSet
            .Include(co => co.Course)
            .Include(co => co.Term)
            .Include(co => co.Teacher)
            .Include(co => co.Section)
            .Where(co => co.CourseId == courseId)
            .OrderByDescending(co => co.Term.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseOffering>> GetByTeacherAsync(int teacherId)
    {
        return await _dbSet
            .Include(co => co.Course)
            .Include(co => co.Term)
            .Include(co => co.Teacher)
            .Include(co => co.Section)
            .Where(co => co.TeacherId == teacherId)
            .OrderByDescending(co => co.Term.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseOffering>> GetBySectionAsync(int sectionId)
    {
        return await _dbSet
            .Include(co => co.Course)
            .Include(co => co.Term)
            .Include(co => co.Teacher)
            .Include(co => co.Section)
            .Where(co => co.SectionId == sectionId)
            .OrderByDescending(co => co.Term.StartDate)
            .ToListAsync();
    }

    public async Task<CourseOffering?> GetOfferingWithEnrollmentsAsync(int offeringId)
    {
        return await _dbSet
            .Include(co => co.Course)
            .Include(co => co.Term)
            .Include(co => co.Teacher)
            .Include(co => co.Section)
            .Include(co => co.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(co => co.Id == offeringId);
    }

    public async Task<CourseOffering?> GetOfferingWithDetailsAsync(int offeringId)
    {
        return await _dbSet
            .Include(co => co.Course)
                .ThenInclude(c => c.Department)
            .Include(co => co.Term)
            .Include(co => co.Teacher)
            .Include(co => co.Section)
                .ThenInclude(s => s.Level)
            .Include(co => co.Sessions)
                .ThenInclude(s => s.Room)
            .Include(co => co.Enrollments)
                .ThenInclude(e => e.Student)
            .Include(co => co.GradeComponents)
            .FirstOrDefaultAsync(co => co.Id == offeringId);
    }

    public async Task<bool> HasScheduleConflictAsync(int studentId, int offeringId)
    {
        // TODO: Implement schedule conflict detection when ScheduledSlot entity is available
        // For now, return false (no conflict) as schedule slots are managed through Sessions
        // Future implementation should check session times for conflicts
        await Task.CompletedTask; // Suppress async warning
        return false;
    }

    public async Task<CourseOffering?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(co => co.Course)
            .Include(co => co.Term)
            .Include(co => co.Teacher)
            .Include(co => co.Section)
            .Include(co => co.Enrollments)
            .FirstOrDefaultAsync(co => co.Id == id);
    }
}
