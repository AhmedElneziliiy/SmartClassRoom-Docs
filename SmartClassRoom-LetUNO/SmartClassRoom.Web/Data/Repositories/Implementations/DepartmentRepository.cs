using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Department entity
/// </summary>
public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Department?> GetWithHeadAsync(int departmentId)
    {
        return await _dbSet
            .Include(d => d.University)
            .Include(d => d.HeadOfDepartment)
            .FirstOrDefaultAsync(d => d.Id == departmentId);
    }

    public async Task<IEnumerable<Department>> GetByUniversityAsync(int universityId)
    {
        return await _dbSet
            .Where(d => d.UniversityId == universityId)
            .Include(d => d.University)
            .Include(d => d.HeadOfDepartment)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
    {
        return await _dbSet
            .Where(d => d.IsActive)
            .Include(d => d.University)
            .Include(d => d.HeadOfDepartment)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetActiveDepartmentsByUniversityAsync(int universityId)
    {
        return await _dbSet
            .Where(d => d.UniversityId == universityId && d.IsActive)
            .Include(d => d.University)
            .Include(d => d.HeadOfDepartment)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetDepartmentWithDetailsAsync(int departmentId)
    {
        return await _dbSet
            .Include(d => d.University)
            .Include(d => d.HeadOfDepartment)
            .Include(d => d.Students)
            .Include(d => d.Teachers)
            .Include(d => d.Courses)
            .Include(d => d.Levels)
            .FirstOrDefaultAsync(d => d.Id == departmentId);
    }

    public async Task<Department?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(d => d.University)
            .Include(d => d.HeadOfDepartment)
            .FirstOrDefaultAsync(d => d.Code == code);
    }

    public async Task<DepartmentStatistics> GetDepartmentStatisticsAsync(int departmentId)
    {
        var department = await _dbSet
            .Include(d => d.Students)
            .Include(d => d.Teachers)
            .Include(d => d.Courses)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
        {
            return new DepartmentStatistics();
        }

        var activeCourseOfferingsCount = await _context.CourseOfferings
            .Where(co => co.Course.DepartmentId == departmentId)
            .CountAsync();

        return new DepartmentStatistics
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            StudentCount = department.Students.Count,
            TeacherCount = department.Teachers.Count,
            CourseCount = department.Courses.Count,
            ActiveCourseOfferingsCount = activeCourseOfferingsCount
        };
    }
}
