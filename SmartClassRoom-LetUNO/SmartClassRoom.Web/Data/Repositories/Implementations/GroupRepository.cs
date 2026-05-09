using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class GroupRepository : Repository<Group>, IGroupRepository
{
    public GroupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Group>> GetBySectionAsync(int sectionId)
    {
        return await _dbSet
            .Where(g => g.SectionId == sectionId)
            .Include(g => g.Section)
                .ThenInclude(s => s.Level)
                    .ThenInclude(l => l.Department)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<Group?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .FirstOrDefaultAsync(g => g.Code == code);
    }

    public async Task<IEnumerable<Group>> GetActiveGroupsBySectionAsync(int sectionId)
    {
        return await _dbSet
            .Where(g => g.SectionId == sectionId && g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<Group?> GetGroupWithDetailsAsync(int groupId)
    {
        return await _dbSet
            .Include(g => g.Section)
                .ThenInclude(s => s.Level)
                    .ThenInclude(l => l.Department)
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<GroupStatistics> GetGroupStatisticsAsync(int groupId)
    {
        var group = await _dbSet
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            return new GroupStatistics();
        }

        return new GroupStatistics
        {
            GroupId = group.Id,
            GroupName = group.Name,
            StudentsCount = group.Students.Count,
            ActiveStudentsCount = group.Students.Count(s => s.IsActive),
            Capacity = group.Capacity
        };
    }
}
