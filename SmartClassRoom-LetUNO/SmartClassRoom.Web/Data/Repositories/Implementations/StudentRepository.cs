using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Student entity
/// </summary>
public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Student>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(s => s.DepartmentId == departmentId)
            .Include(s => s.Department)
            .Include(s => s.Level)
            .Include(s => s.Section)
            .Include(s => s.Group)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByLevelAsync(int levelId)
    {
        return await _dbSet
            .Where(s => s.LevelId == levelId)
            .Include(s => s.Department)
            .Include(s => s.Level)
            .Include(s => s.Section)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetBySectionAsync(int sectionId)
    {
        return await _dbSet
            .Where(s => s.SectionId == sectionId)
            .Include(s => s.Department)
            .Include(s => s.Level)
            .Include(s => s.Section)
            .Include(s => s.Group)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetByGroupAsync(int groupId)
    {
        return await _dbSet
            .Where(s => s.GroupId == groupId)
            .Include(s => s.Department)
            .Include(s => s.Level)
            .Include(s => s.Section)
            .Include(s => s.Group)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .Include(s => s.Department)
            .Include(s => s.Level)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetActiveStudentsByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(s => s.DepartmentId == departmentId && s.IsActive)
            .Include(s => s.Department)
            .Include(s => s.Level)
            .Include(s => s.Section)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> SearchByNameOrCodeAsync(string searchTerm)
    {
        var search = searchTerm.ToLower();

        return await _dbSet
            .Where(s =>
                s.FullName.ToLower().Contains(search) ||
                (s.StudentCode != null && s.StudentCode.ToLower().Contains(search)))
            .Include(s => s.Department)
            .Include(s => s.Level)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<Student?> GetStudentWithDetailsAsync(int studentId)
    {
        return await _dbSet
            .Include(s => s.Department)
            .Include(s => s.Level)
            .Include(s => s.Section)
            .Include(s => s.Group)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
            .FirstOrDefaultAsync(s => s.Id == studentId);
    }

    public async Task<IEnumerable<Student>> GetStudentsByCourseOfferingAsync(int courseOfferingId)
    {
        return await _context.StudentEnrollments
            .Where(e => e.CourseOfferingId == courseOfferingId)
            .Include(e => e.Student)
                .ThenInclude(s => s.Department)
            .Include(e => e.Student)
                .ThenInclude(s => s.Level)
            .Select(e => e.Student)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<Student?> GetByStudentCodeAsync(string studentCode)
    {
        return await _dbSet
            .Include(s => s.Department)
            .Include(s => s.Level)
            .FirstOrDefaultAsync(s => s.StudentCode == studentCode);
    }
}
