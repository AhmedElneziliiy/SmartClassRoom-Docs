using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface ITermRepository : IRepository<Term>
{
    Task<Term?> GetCurrentTermAsync();
    Task<Term?> GetUpcomingTermAsync();
    Task<IEnumerable<Term>> GetActiveTermsAsync();
    Task<Term?> GetTermWithOfferingsAsync(int termId);
    Task<TermStatistics> GetTermStatisticsAsync(int termId);
    Task<Term?> GetByCodeAsync(string code);
}
