using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.ViewModels.Universities;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service implementation for managing University entities
/// </summary>
public class UniversityService : IUniversityService
{
    private readonly IUnitOfWork _unitOfWork;

    public UniversityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<University>> GetUniversitiesPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Get all universities with departments
        var allUniversities = await _unitOfWork.Repository<University>()
            .GetAllAsync("Departments");

        // Apply filtering
        IEnumerable<University> filtered = allUniversities;
        if (isActive.HasValue)
        {
            filtered = filtered.Where(u => u.IsActive == isActive.Value);
        }

        // Apply sorting
        IEnumerable<University> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("name", true) => filtered.OrderBy(u => u.Name),
            ("name", false) => filtered.OrderByDescending(u => u.Name),
            ("code", true) => filtered.OrderBy(u => u.Code ?? string.Empty),
            ("code", false) => filtered.OrderByDescending(u => u.Code ?? string.Empty),
            ("status", true) => filtered.OrderBy(u => u.IsActive),
            ("status", false) => filtered.OrderByDescending(u => u.IsActive),
            ("created", true) => filtered.OrderBy(u => u.CreatedAt),
            ("created", false) => filtered.OrderByDescending(u => u.CreatedAt),
            _ => filtered.OrderBy(u => u.Name) // Default sort
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<University>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<University?> GetUniversityByIdAsync(int id)
    {
        return await _unitOfWork.Repository<University>()
            .GetByIdAsync(id);
    }

    public async Task<University?> GetUniversityWithDepartmentsAsync(int id)
    {
        return await _unitOfWork.Repository<University>()
            .GetByIdAsync(id, "Departments", "Departments.Students", "Departments.Teachers", "Departments.HeadOfDepartment");
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var universities = await _unitOfWork.Repository<University>()
            .FindAsync(u => u.Code == code);

        var existing = universities.FirstOrDefault();

        if (existing == null)
            return false;

        if (excludeId.HasValue && existing.Id == excludeId.Value)
            return false;

        return true;
    }

    public async Task<(bool Success, string Message)> CreateUniversityAsync(CreateUniversityRequest request)
    {
        try
        {
            // Validate unique code
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                if (await CodeExistsAsync(request.Code))
                {
                    return (false, $"University code '{request.Code}' already exists.");
                }
            }

            var university = new University
            {
                Name = request.Name,
                Code = request.Code,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                Website = request.Website,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<University>().AddAsync(university);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"University '{university.Name}' created successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating university: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateUniversityAsync(EditUniversityRequest request)
    {
        try
        {
            var university = await GetUniversityByIdAsync(request.Id);
            if (university == null)
            {
                return (false, "University not found.");
            }

            // Validate unique code
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                if (await CodeExistsAsync(request.Code, request.Id))
                {
                    return (false, $"University code '{request.Code}' already exists.");
                }
            }

            // Update properties
            university.Name = request.Name;
            university.Code = request.Code;
            university.Address = request.Address;
            university.Phone = request.Phone;
            university.Email = request.Email;
            university.Website = request.Website;
            university.IsActive = request.IsActive;

            _unitOfWork.Repository<University>().Update(university);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"University '{university.Name}' updated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating university: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeactivateUniversityAsync(int id)
    {
        try
        {
            var university = await _unitOfWork.Repository<University>()
                .GetByIdAsync(id, "Departments");

            if (university == null)
            {
                return (false, "University not found.");
            }

            if (!university.IsActive)
            {
                return (false, "University is already deactivated.");
            }

            // Validation: Check for active departments
            var activeDepartments = university.Departments.Count(d => d.IsActive);
            if (activeDepartments > 0)
            {
                return (false, $"Cannot deactivate university. It has {activeDepartments} active department(s). Please deactivate all departments first.");
            }

            university.IsActive = false;
            _unitOfWork.Repository<University>().Update(university);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"University '{university.Name}' deactivated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error deactivating university: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ActivateUniversityAsync(int id)
    {
        try
        {
            var university = await GetUniversityByIdAsync(id);
            if (university == null)
            {
                return (false, "University not found.");
            }

            if (university.IsActive)
            {
                return (false, "University is already active.");
            }

            university.IsActive = true;
            _unitOfWork.Repository<University>().Update(university);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"University '{university.Name}' activated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error activating university: {ex.Message}");
        }
    }

    public async Task<UniversityStatistics> GetUniversityStatisticsAsync(int id)
    {
        var university = await _unitOfWork.Repository<University>()
            .GetByIdAsync(id, "Departments", "Departments.Students", "Departments.Teachers");

        if (university == null)
        {
            return new UniversityStatistics
            {
                UniversityId = id,
                UniversityName = "Unknown"
            };
        }

        return new UniversityStatistics
        {
            UniversityId = university.Id,
            UniversityName = university.Name,
            DepartmentCount = university.Departments.Count,
            ActiveDepartmentCount = university.Departments.Count(d => d.IsActive),
            TotalStudentCount = university.Departments.Sum(d => d.Students.Count),
            TotalTeacherCount = university.Departments.Sum(d => d.Teachers.Count)
        };
    }
}
