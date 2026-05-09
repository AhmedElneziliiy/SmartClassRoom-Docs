using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Terms;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class TermService : ITermService
{
    private readonly IUnitOfWork _unitOfWork;

    public TermService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Term>> GetTermsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        bool? isActive = null,
        string? status = null,
        string? sortBy = null,
        bool ascending = true)
    {
        var allTerms = await _unitOfWork.Terms.GetAllAsync("CourseOfferings");

        // Apply filtering
        IEnumerable<Term> filtered = allTerms;

        if (isActive.HasValue)
        {
            filtered = filtered.Where(t => t.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(t => t.Status == status);
        }

        // Apply sorting
        IEnumerable<Term> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("name", true) => filtered.OrderBy(t => t.Name),
            ("name", false) => filtered.OrderByDescending(t => t.Name),
            ("code", true) => filtered.OrderBy(t => t.Code),
            ("code", false) => filtered.OrderByDescending(t => t.Code),
            ("startdate", true) => filtered.OrderBy(t => t.StartDate),
            ("startdate", false) => filtered.OrderByDescending(t => t.StartDate),
            ("enddate", true) => filtered.OrderBy(t => t.EndDate),
            ("enddate", false) => filtered.OrderByDescending(t => t.EndDate),
            ("status", true) => filtered.OrderBy(t => t.Status),
            ("status", false) => filtered.OrderByDescending(t => t.Status),
            _ => filtered.OrderByDescending(t => t.StartDate) // Default: newest first
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Term>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Term?> GetTermByIdAsync(int id)
    {
        return await _unitOfWork.Terms.GetByIdAsync(id);
    }

    public async Task<Term?> GetTermWithDetailsAsync(int id)
    {
        return await _unitOfWork.Terms.GetTermWithOfferingsAsync(id);
    }

    public async Task<Term?> GetCurrentTermAsync()
    {
        return await _unitOfWork.Terms.GetCurrentTermAsync();
    }

    public async Task<Term?> GetUpcomingTermAsync()
    {
        return await _unitOfWork.Terms.GetUpcomingTermAsync();
    }

    public async Task<TermStatistics> GetTermStatisticsAsync(int id)
    {
        return await _unitOfWork.Terms.GetTermStatisticsAsync(id);
    }

    public async Task<(bool Success, string Message)> CreateTermAsync(CreateTermRequest request)
    {
        // Validate dates
        if (request.StartDate >= request.EndDate)
        {
            return (false, "Start date must be before end date.");
        }

        // Check code uniqueness
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            if (await CodeExistsAsync(request.Code))
            {
                return (false, $"Term code '{request.Code}' already exists.");
            }
        }

        // Check for overlapping active terms
        var activeTerms = await _unitOfWork.Terms.GetActiveTermsAsync();
        var hasOverlap = activeTerms.Any(t =>
            (request.StartDate >= t.StartDate && request.StartDate <= t.EndDate) ||
            (request.EndDate >= t.StartDate && request.EndDate <= t.EndDate) ||
            (request.StartDate <= t.StartDate && request.EndDate >= t.EndDate));

        if (hasOverlap)
        {
            return (false, "Term dates overlap with an existing active term.");
        }

        var term = new Term
        {
            Name = request.Name,
            Code = request.Code,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            IsActive = request.IsActive
        };

        await _unitOfWork.Terms.AddAsync(term);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Term created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateTermAsync(EditTermRequest request)
    {
        var term = await _unitOfWork.Terms.GetByIdAsync(request.Id);
        if (term == null)
        {
            return (false, "Term not found.");
        }

        // Validate dates
        if (request.StartDate >= request.EndDate)
        {
            return (false, "Start date must be before end date.");
        }

        // Check code uniqueness (excluding current term)
        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != term.Code)
        {
            if (await CodeExistsAsync(request.Code, request.Id))
            {
                return (false, $"Term code '{request.Code}' already exists.");
            }
        }

        // Update properties
        term.Name = request.Name;
        term.Code = request.Code;
        term.StartDate = request.StartDate;
        term.EndDate = request.EndDate;
        term.Status = request.Status;
        term.IsActive = request.IsActive;

        _unitOfWork.Terms.Update(term);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Term updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeactivateTermAsync(int id)
    {
        var term = await _unitOfWork.Terms.GetTermWithOfferingsAsync(id);
        if (term == null)
        {
            return (false, "Term not found.");
        }

        if (!term.IsActive)
        {
            return (false, "Term is already inactive.");
        }

        // Check if term has active course offerings
        var hasActiveOfferings = term.CourseOfferings.Any(co => co.Status == "Active");
        if (hasActiveOfferings)
        {
            return (false, "Cannot deactivate term with active course offerings.");
        }

        term.IsActive = false;
        _unitOfWork.Terms.Update(term);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Term deactivated successfully.");
    }

    public async Task<(bool Success, string Message)> ActivateTermAsync(int id)
    {
        var term = await _unitOfWork.Terms.GetByIdAsync(id);
        if (term == null)
        {
            return (false, "Term not found.");
        }

        if (term.IsActive)
        {
            return (false, "Term is already active.");
        }

        term.IsActive = true;
        _unitOfWork.Terms.Update(term);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Term activated successfully.");
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var term = await _unitOfWork.Terms.GetByCodeAsync(code);
        if (term == null)
            return false;

        if (excludeId.HasValue && term.Id == excludeId.Value)
            return false;

        return true;
    }
}
