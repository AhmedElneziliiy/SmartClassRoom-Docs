using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IGroupRepository : IRepository<Group>
{
    Task<IEnumerable<Group>> GetBySectionAsync(int sectionId);
    Task<Group?> GetByCodeAsync(string code);
    Task<IEnumerable<Group>> GetActiveGroupsBySectionAsync(int sectionId);
    Task<Group?> GetGroupWithDetailsAsync(int groupId);
    Task<GroupStatistics> GetGroupStatisticsAsync(int groupId);
}
