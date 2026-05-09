using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Course entity
/// </summary>
public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Course>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(c => c.DepartmentId == departmentId)
            .Include(c => c.Department)
            .Include(c => c.PrerequisiteCourse)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<Course?> GetWithPrerequisitesAsync(int courseId)
    {
        return await _dbSet
            .Include(c => c.Department)
            .Include(c => c.PrerequisiteCourse)
            .Include(c => c.PrerequisiteFor)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }

    public async Task<IEnumerable<Course>> GetCoursesRequiringPrerequisiteAsync(int prerequisiteCourseId)
    {
        return await _dbSet
            .Where(c => c.PrerequisiteCourseId == prerequisiteCourseId)
            .Include(c => c.Department)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> SearchByNameOrCodeAsync(string searchTerm)
    {
        var search = searchTerm.ToLower();

        return await _dbSet
            .Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search))
            .Include(c => c.Department)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<Course?> GetByCourseCodeAsync(string courseCode)
    {
        return await _dbSet
            .Include(c => c.Department)
            .Include(c => c.PrerequisiteCourse)
            .FirstOrDefaultAsync(c => c.Code == courseCode);
    }

    public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .Include(c => c.Department)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetActiveCoursesByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(c => c.DepartmentId == departmentId && c.IsActive)
            .Include(c => c.Department)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetByCourseTypeAsync(string courseType)
    {
        return await _dbSet
            .Where(c => c.CourseType == courseType)
            .Include(c => c.Department)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<Course?> GetCourseWithOfferingsAsync(int courseId)
    {
        return await _dbSet
            .Include(c => c.Department)
            .Include(c => c.Offerings)
                .ThenInclude(o => o.Teacher)
            .Include(c => c.Offerings)
                .ThenInclude(o => o.Term)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }
}
