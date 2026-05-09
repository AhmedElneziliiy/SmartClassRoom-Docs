using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Grading;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class GradeComponentRepository : Repository<GradeComponent>, IGradeComponentRepository
{
    public GradeComponentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<GradeComponent>> GetComponentsByOfferingAsync(int courseOfferingId)
    {
        return await _context.GradeComponents
            .Where(gc => gc.CourseOfferingId == courseOfferingId)
            .OrderBy(gc => gc.OrderIndex)
            .ToListAsync();
    }

    public async Task<GradeComponent?> GetComponentWithGradesAsync(int componentId)
    {
        return await _context.GradeComponents
            .Include(gc => gc.Grades)
                .ThenInclude(g => g.Student)
            .FirstOrDefaultAsync(gc => gc.Id == componentId);
    }

    public async Task<decimal> GetTotalWeightAsync(int courseOfferingId)
    {
        return await _context.GradeComponents
            .Where(gc => gc.CourseOfferingId == courseOfferingId)
            .SumAsync(gc => gc.Weight);
    }

    public async Task DeleteComponentsByOfferingAsync(int courseOfferingId)
    {
        // First, delete all grades associated with components in this offering
        var grades = await _context.Grades
            .Where(g => g.CourseOfferingId == courseOfferingId)
            .ToListAsync();

        if (grades.Any())
        {
            _context.Grades.RemoveRange(grades);
        }

        // Then delete the components
        var components = await _context.GradeComponents
            .Where(gc => gc.CourseOfferingId == courseOfferingId)
            .ToListAsync();

        if (components.Any())
        {
            _context.GradeComponents.RemoveRange(components);
        }
    }

    public async Task<int> GetComponentCountAsync(int courseOfferingId)
    {
        return await _context.GradeComponents
            .CountAsync(gc => gc.CourseOfferingId == courseOfferingId);
    }
}
