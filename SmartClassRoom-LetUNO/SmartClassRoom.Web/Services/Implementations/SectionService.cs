using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Sections;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service implementation for managing Section entities
/// </summary>
public class SectionService : ISectionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SectionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Section>> GetSectionsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? levelId = null,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Get all sections with related data
        var allSections = await _unitOfWork.Sections
            .GetAllAsync("Level", "Level.Department", "Groups", "Students");

        // Apply filtering
        IEnumerable<Section> filtered = allSections;

        if (levelId.HasValue)
        {
            filtered = filtered.Where(s => s.LevelId == levelId.Value);
        }

        if (departmentId.HasValue)
        {
            filtered = filtered.Where(s => s.Level.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            filtered = filtered.Where(s => s.IsActive == isActive.Value);
        }

        // Apply sorting
        IEnumerable<Section> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("name", true) => filtered.OrderBy(s => s.Name),
            ("name", false) => filtered.OrderByDescending(s => s.Name),
            ("code", true) => filtered.OrderBy(s => s.Code ?? string.Empty),
            ("code", false) => filtered.OrderByDescending(s => s.Code ?? string.Empty),
            ("level", true) => filtered.OrderBy(s => s.Level.Name),
            ("level", false) => filtered.OrderByDescending(s => s.Level.Name),
            ("capacity", true) => filtered.OrderBy(s => s.Capacity ?? 0),
            ("capacity", false) => filtered.OrderByDescending(s => s.Capacity ?? 0),
            ("status", true) => filtered.OrderBy(s => s.IsActive),
            ("status", false) => filtered.OrderByDescending(s => s.IsActive),
            _ => filtered.OrderBy(s => s.Name) // Default sort
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<Section>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Section?> GetSectionByIdAsync(int id)
    {
        return await _unitOfWork.Sections.GetByIdAsync(id);
    }

    public async Task<Section?> GetSectionWithDetailsAsync(int id)
    {
        return await _unitOfWork.Sections.GetSectionWithDetailsAsync(id);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var existing = await _unitOfWork.Sections.GetByCodeAsync(code);

        if (existing == null)
            return false;

        if (excludeId.HasValue && existing.Id == excludeId.Value)
            return false;

        return true;
    }

    public async Task<(bool Success, string Message)> CreateSectionAsync(CreateSectionRequest request)
    {
        try
        {
            // Validate level exists
            var level = await _unitOfWork.Repository<Level>()
                .GetByIdAsync(request.LevelId, "Department");

            if (level == null)
            {
                return (false, "Level not found.");
            }

            // Validate level is active
            if (!level.IsActive)
            {
                return (false, "Cannot create section. Level is inactive.");
            }

            // Validate department is active
            if (!level.Department.IsActive)
            {
                return (false, "Cannot create section. Parent department is inactive.");
            }

            // Validate unique code (if provided)
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                if (await CodeExistsAsync(request.Code))
                {
                    return (false, $"Section code '{request.Code}' already exists.");
                }
            }

            // Validate capacity is positive (if provided)
            if (request.Capacity.HasValue && request.Capacity.Value <= 0)
            {
                return (false, "Capacity must be greater than zero.");
            }

            var section = new Section
            {
                Name = request.Name,
                Code = request.Code,
                LevelId = request.LevelId,
                Capacity = request.Capacity,
                IsActive = request.IsActive
            };

            await _unitOfWork.Sections.AddAsync(section);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Section '{section.Name}' created successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating section: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateSectionAsync(EditSectionRequest request)
    {
        try
        {
            var section = await _unitOfWork.Sections
                .GetByIdAsync(request.Id, "Students");

            if (section == null)
            {
                return (false, "Section not found.");
            }

            // Validate level exists
            var level = await _unitOfWork.Repository<Level>()
                .GetByIdAsync(request.LevelId, "Department");

            if (level == null)
            {
                return (false, "Level not found.");
            }

            // Validate level is active
            if (!level.IsActive)
            {
                return (false, "Cannot update section. Level is inactive.");
            }

            // Validate department is active
            if (!level.Department.IsActive)
            {
                return (false, "Cannot update section. Parent department is inactive.");
            }

            // Validate unique code (if provided)
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                if (await CodeExistsAsync(request.Code, request.Id))
                {
                    return (false, $"Section code '{request.Code}' already exists.");
                }
            }

            // Validate capacity (if provided)
            if (request.Capacity.HasValue)
            {
                if (request.Capacity.Value <= 0)
                {
                    return (false, "Capacity must be greater than zero.");
                }

                // Check if capacity is less than current students count
                var currentStudentsCount = section.Students.Count;
                if (request.Capacity.Value < currentStudentsCount)
                {
                    return (false, $"Cannot set capacity to {request.Capacity.Value}. Section has {currentStudentsCount} student(s).");
                }
            }

            // Update properties
            section.Name = request.Name;
            section.Code = request.Code;
            section.LevelId = request.LevelId;
            section.Capacity = request.Capacity;
            section.IsActive = request.IsActive;

            _unitOfWork.Sections.Update(section);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Section '{section.Name}' updated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating section: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeactivateSectionAsync(int id)
    {
        try
        {
            var section = await _unitOfWork.Sections
                .GetByIdAsync(id, "Groups", "Students", "CourseOfferings");

            if (section == null)
            {
                return (false, "Section not found.");
            }

            if (!section.IsActive)
            {
                return (false, "Section is already deactivated.");
            }

            // Validation: Check for active groups
            var activeGroups = section.Groups.Count(g => g.IsActive);
            if (activeGroups > 0)
            {
                return (false, $"Cannot deactivate section. It has {activeGroups} active group(s).");
            }

            // Validation: Check for active students
            var activeStudents = section.Students.Count(s => s.IsActive);
            if (activeStudents > 0)
            {
                return (false, $"Cannot deactivate section. It has {activeStudents} active student(s).");
            }

            // Validation: Check for course offerings
            if (section.CourseOfferings.Any())
            {
                return (false, $"Cannot deactivate section. It has {section.CourseOfferings.Count} course offering(s). Please remove or reassign course offerings first.");
            }

            section.IsActive = false;
            _unitOfWork.Sections.Update(section);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Section '{section.Name}' deactivated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error deactivating section: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ActivateSectionAsync(int id)
    {
        try
        {
            var section = await _unitOfWork.Sections
                .GetByIdAsync(id, "Level", "Level.Department");

            if (section == null)
            {
                return (false, "Section not found.");
            }

            if (section.IsActive)
            {
                return (false, "Section is already active.");
            }

            // Validation: Check if parent level is active
            if (!section.Level.IsActive)
            {
                return (false, $"Cannot activate section. Parent level '{section.Level.Name}' is inactive.");
            }

            // Validation: Check if parent department is active
            if (!section.Level.Department.IsActive)
            {
                return (false, $"Cannot activate section. Parent department '{section.Level.Department.Name}' is inactive.");
            }

            section.IsActive = true;
            _unitOfWork.Sections.Update(section);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Section '{section.Name}' activated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error activating section: {ex.Message}");
        }
    }

    public async Task<SectionStatistics> GetSectionStatisticsAsync(int id)
    {
        return await _unitOfWork.Sections.GetSectionStatisticsAsync(id);
    }
}
