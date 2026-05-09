using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Section entity
/// </summary>
public class SectionRepository : Repository<Section>, ISectionRepository
{
    public SectionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Section>> GetByLevelAsync(int levelId)
    {
        return await _dbSet
            .Where(s => s.LevelId == levelId)
            .Include(s => s.Level)
                .ThenInclude(l => l.Department)
            .Include(s => s.Groups)
            .Include(s => s.Students)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Section?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return await _dbSet
            .Include(s => s.Level)
            .FirstOrDefaultAsync(s => s.Code == code);
    }

    public async Task<IEnumerable<Section>> GetActiveSectionsByLevelAsync(int levelId)
    {
        return await _dbSet
            .Where(s => s.LevelId == levelId && s.IsActive)
            .Include(s => s.Level)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Section?> GetSectionWithDetailsAsync(int sectionId)
    {
        return await _dbSet
            .Include(s => s.Level)
                .ThenInclude(l => l.Department)
                    .ThenInclude(d => d.University)
            .Include(s => s.Groups)
            .Include(s => s.Students)
            .Include(s => s.CourseOfferings)
            .FirstOrDefaultAsync(s => s.Id == sectionId);
    }

    public async Task<SectionStatistics> GetSectionStatisticsAsync(int sectionId)
    {
        var section = await _dbSet
            .Include(s => s.Groups)
            .Include(s => s.Students)
            .Include(s => s.CourseOfferings)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        if (section == null)
        {
            return new SectionStatistics();
        }

        return new SectionStatistics
        {
            SectionId = section.Id,
            SectionName = section.Name,
            SectionCode = section.Code ?? string.Empty,
            GroupsCount = section.Groups.Count,
            ActiveGroupsCount = section.Groups.Count(g => g.IsActive),
            StudentsCount = section.Students.Count,
            ActiveStudentsCount = section.Students.Count(s => s.IsActive),
            CourseOfferingsCount = section.CourseOfferings.Count,
            Capacity = section.Capacity
        };
    }
}
