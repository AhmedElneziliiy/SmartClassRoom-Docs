using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Models.ViewModels.Groups;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface IGroupService
{
    Task<PagedResult<Group>> GetGroupsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? sectionId = null,
        int? levelId = null,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    Task<Group?> GetGroupByIdAsync(int id);
    Task<Group?> GetGroupWithDetailsAsync(int id);
    Task<GroupStatistics> GetGroupStatisticsAsync(int id);
    Task<(bool Success, string Message)> CreateGroupAsync(CreateGroupRequest request);
    Task<(bool Success, string Message)> UpdateGroupAsync(EditGroupRequest request);
    Task<(bool Success, string Message)> DeactivateGroupAsync(int id);
    Task<(bool Success, string Message)> ActivateGroupAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
}
