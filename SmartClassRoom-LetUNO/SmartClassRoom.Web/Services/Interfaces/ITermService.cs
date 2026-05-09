using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Terms;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface ITermService
{
    Task<PagedResult<Term>> GetTermsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        bool? isActive = null,
        string? status = null,
        string? sortBy = null,
        bool ascending = true);

    Task<Term?> GetTermByIdAsync(int id);
    Task<Term?> GetTermWithDetailsAsync(int id);
    Task<Term?> GetCurrentTermAsync();
    Task<Term?> GetUpcomingTermAsync();
    Task<TermStatistics> GetTermStatisticsAsync(int id);
    Task<(bool Success, string Message)> CreateTermAsync(CreateTermRequest request);
    Task<(bool Success, string Message)> UpdateTermAsync(EditTermRequest request);
    Task<(bool Success, string Message)> DeactivateTermAsync(int id);
    Task<(bool Success, string Message)> ActivateTermAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
}
