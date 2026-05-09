using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Grading;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class GradeRepository : Repository<Grade>, IGradeRepository
{
    public GradeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Grade>> GetGradesByStudentAndOfferingAsync(int studentId, int courseOfferingId)
    {
        return await _context.Grades
            .Include(g => g.GradeComponent)
            .Include(g => g.Student)
            .Where(g => g.StudentId == studentId && g.CourseOfferingId == courseOfferingId)
            .OrderBy(g => g.GradeComponent.OrderIndex)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grade>> GetGradesByComponentAsync(int gradeComponentId)
    {
        return await _context.Grades
            .Include(g => g.Student)
            .Where(g => g.GradeComponentId == gradeComponentId)
            .OrderBy(g => g.Student.FullName)
            .ToListAsync();
    }

    public async Task<Grade?> GetGradeAsync(int studentId, int courseOfferingId, int gradeComponentId)
    {
        return await _context.Grades
            .Include(g => g.GradeComponent)
            .Include(g => g.Student)
            .FirstOrDefaultAsync(g =>
                g.StudentId == studentId &&
                g.CourseOfferingId == courseOfferingId &&
                g.GradeComponentId == gradeComponentId);
    }

    public async Task<IEnumerable<Grade>> GetGradesByOfferingAsync(int courseOfferingId)
    {
        return await _context.Grades
            .Include(g => g.Student)
            .Include(g => g.GradeComponent)
            .Where(g => g.CourseOfferingId == courseOfferingId)
            .OrderBy(g => g.Student.FullName)
            .ThenBy(g => g.GradeComponent.OrderIndex)
            .ToListAsync();
    }

    public async Task<bool> GradeExistsAsync(int studentId, int courseOfferingId, int gradeComponentId)
    {
        return await _context.Grades
            .AnyAsync(g =>
                g.StudentId == studentId &&
                g.CourseOfferingId == courseOfferingId &&
                g.GradeComponentId == gradeComponentId);
    }
}
