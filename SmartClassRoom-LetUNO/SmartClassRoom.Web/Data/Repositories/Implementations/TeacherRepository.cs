using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Teacher entity
/// </summary>
public class TeacherRepository : Repository<Teacher>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Teacher>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(t => t.DepartmentId == departmentId)
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetActiveTeachersAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetActiveTeachersByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(t => t.DepartmentId == departmentId && t.IsActive)
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> SearchByNameOrCodeAsync(string searchTerm)
    {
        var search = searchTerm.ToLower();

        return await _dbSet
            .Where(t =>
                t.FullName.ToLower().Contains(search) ||
                (t.EmployeeCode != null && t.EmployeeCode.ToLower().Contains(search)))
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId)
    {
        return await _dbSet
            .Include(t => t.Department)
            .Include(t => t.CourseOfferings)
                .ThenInclude(co => co.Course)
            .FirstOrDefaultAsync(t => t.Id == teacherId);
    }

    public async Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode)
    {
        return await _dbSet
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.EmployeeCode == employeeCode);
    }

    public async Task<IEnumerable<Teacher>> GetTeachersByCourseAsync(int courseId)
    {
        return await _context.CourseOfferings
            .Where(co => co.CourseId == courseId)
            .Include(co => co.Teacher)
                .ThenInclude(t => t.Department)
            .Select(co => co.Teacher)
            .Distinct()
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Teacher>> GetByEmploymentStatusAsync(string employmentStatus)
    {
        return await _dbSet
            .Where(t => t.EmploymentStatus == employmentStatus)
            .Include(t => t.Department)
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }
}
