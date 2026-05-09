using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class TermRepository : Repository<Term>, ITermRepository
{
    public TermRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Term?> GetCurrentTermAsync()
    {
        var today = DateTime.UtcNow;

        // Find term where Status is "Active" OR current date is within term dates
        return await _dbSet
            .Where(t => t.IsActive)
            .Where(t => t.Status == "Active" || (t.StartDate <= today && t.EndDate >= today))
            .OrderBy(t => t.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Term?> GetUpcomingTermAsync()
    {
        var today = DateTime.UtcNow;

        // Find term where Status is "Upcoming" OR start date is in the future
        return await _dbSet
            .Where(t => t.IsActive)
            .Where(t => t.Status == "Upcoming" || t.StartDate > today)
            .OrderBy(t => t.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Term>> GetActiveTermsAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
    }

    public async Task<Term?> GetTermWithOfferingsAsync(int termId)
    {
        return await _dbSet
            .Include(t => t.CourseOfferings)
                .ThenInclude(co => co.Course)
            .Include(t => t.CourseOfferings)
                .ThenInclude(co => co.Teacher)
            .Include(t => t.CourseOfferings)
                .ThenInclude(co => co.Enrollments)
            .FirstOrDefaultAsync(t => t.Id == termId);
    }

    public async Task<TermStatistics> GetTermStatisticsAsync(int termId)
    {
        var term = await _dbSet
            .Include(t => t.CourseOfferings)
                .ThenInclude(co => co.Enrollments)
            .FirstOrDefaultAsync(t => t.Id == termId);

        if (term == null)
        {
            return new TermStatistics();
        }

        var activeOfferings = term.CourseOfferings.Where(co => co.Status == "Active").ToList();
        var totalEnrollments = term.CourseOfferings.Sum(co => co.Enrollments.Count);
        var activeEnrollments = term.CourseOfferings.Sum(co => co.Enrollments.Count(e => e.Status == "Enrolled"));

        return new TermStatistics
        {
            TermId = term.Id,
            TermName = term.Name,
            TermCode = term.Code,
            OfferingsCount = term.CourseOfferings.Count,
            ActiveOfferingsCount = activeOfferings.Count,
            EnrolledStudentsCount = totalEnrollments,
            ActiveEnrollmentsCount = activeEnrollments
        };
    }

    public async Task<Term?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Code == code);
    }
}
