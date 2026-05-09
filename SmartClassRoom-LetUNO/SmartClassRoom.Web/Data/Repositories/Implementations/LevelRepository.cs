using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Level entity
/// </summary>
public class LevelRepository : Repository<Level>, ILevelRepository
{
    public LevelRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Level>> GetByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(l => l.DepartmentId == departmentId)
            .Include(l => l.Department)
            .Include(l => l.Sections)
            .Include(l => l.Students)
            .OrderBy(l => l.LevelNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Level>> GetActiveLevelsByDepartmentAsync(int departmentId)
    {
        return await _dbSet
            .Where(l => l.DepartmentId == departmentId && l.IsActive)
            .Include(l => l.Department)
            .OrderBy(l => l.LevelNumber)
            .ToListAsync();
    }

    public async Task<Level?> GetLevelWithDetailsAsync(int levelId)
    {
        return await _dbSet
            .Include(l => l.Department)
                .ThenInclude(d => d.University)
            .Include(l => l.Sections)
            .Include(l => l.Students)
            .FirstOrDefaultAsync(l => l.Id == levelId);
    }

    public async Task<LevelStatistics> GetLevelStatisticsAsync(int levelId)
    {
        var level = await _dbSet
            .Include(l => l.Sections)
            .Include(l => l.Students)
            .FirstOrDefaultAsync(l => l.Id == levelId);

        if (level == null)
        {
            return new LevelStatistics();
        }

        return new LevelStatistics
        {
            LevelId = level.Id,
            LevelName = level.Name,
            LevelNumber = level.LevelNumber,
            SectionsCount = level.Sections.Count,
            ActiveSectionsCount = level.Sections.Count(s => s.IsActive),
            StudentsCount = level.Students.Count,
            ActiveStudentsCount = level.Students.Count(s => s.IsActive)
        };
    }
}
