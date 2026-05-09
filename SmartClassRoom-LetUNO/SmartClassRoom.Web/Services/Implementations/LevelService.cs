using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Levels;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service implementation for managing Level entities
/// </summary>
public class LevelService : ILevelService
{
    private readonly IUnitOfWork _unitOfWork;

    public LevelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Level>> GetLevelsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Get all levels with related data
        var allLevels = await _unitOfWork.Levels
            .GetAllAsync("Department", "Sections", "Students");

        // Apply filtering
        IEnumerable<Level> filtered = allLevels;

        if (departmentId.HasValue)
        {
            filtered = filtered.Where(l => l.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            filtered = filtered.Where(l => l.IsActive == isActive.Value);
        }

        // Apply sorting
        IEnumerable<Level> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("name", true) => filtered.OrderBy(l => l.Name),
            ("name", false) => filtered.OrderByDescending(l => l.Name),
            ("levelnumber", true) => filtered.OrderBy(l => l.LevelNumber),
            ("levelnumber", false) => filtered.OrderByDescending(l => l.LevelNumber),
            ("department", true) => filtered.OrderBy(l => l.Department.Name),
            ("department", false) => filtered.OrderByDescending(l => l.Department.Name),
            ("status", true) => filtered.OrderBy(l => l.IsActive),
            ("status", false) => filtered.OrderByDescending(l => l.IsActive),
            _ => filtered.OrderBy(l => l.LevelNumber) // Default sort by level number
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<Level>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Level?> GetLevelByIdAsync(int id)
    {
        return await _unitOfWork.Levels.GetByIdAsync(id);
    }

    public async Task<Level?> GetLevelWithDetailsAsync(int id)
    {
        return await _unitOfWork.Levels.GetLevelWithDetailsAsync(id);
    }

    public async Task<(bool Success, string Message)> CreateLevelAsync(CreateLevelRequest request)
    {
        try
        {
            // Validate department exists
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId);
            if (department == null)
            {
                return (false, "Department not found.");
            }

            // Validate department is active
            if (!department.IsActive)
            {
                return (false, "Cannot create level. Department is inactive.");
            }

            // Validate level number is positive
            if (request.LevelNumber <= 0)
            {
                return (false, "Level Number must be greater than zero.");
            }

            var level = new Level
            {
                Name = request.Name,
                LevelNumber = request.LevelNumber,
                DepartmentId = request.DepartmentId,
                IsActive = request.IsActive
            };

            await _unitOfWork.Levels.AddAsync(level);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Level '{level.Name}' created successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating level: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateLevelAsync(EditLevelRequest request)
    {
        try
        {
            var level = await GetLevelByIdAsync(request.Id);
            if (level == null)
            {
                return (false, "Level not found.");
            }

            // Validate department exists
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId);
            if (department == null)
            {
                return (false, "Department not found.");
            }

            // Validate department is active
            if (!department.IsActive)
            {
                return (false, "Cannot update level. Department is inactive.");
            }

            // Validate level number is positive
            if (request.LevelNumber <= 0)
            {
                return (false, "Level Number must be greater than zero.");
            }

            // Update properties
            level.Name = request.Name;
            level.LevelNumber = request.LevelNumber;
            level.DepartmentId = request.DepartmentId;
            level.IsActive = request.IsActive;

            _unitOfWork.Levels.Update(level);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Level '{level.Name}' updated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating level: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeactivateLevelAsync(int id)
    {
        try
        {
            var level = await _unitOfWork.Levels
                .GetByIdAsync(id, "Sections", "Students");

            if (level == null)
            {
                return (false, "Level not found.");
            }

            if (!level.IsActive)
            {
                return (false, "Level is already deactivated.");
            }

            // Validation: Check for active sections
            var activeSections = level.Sections.Count(s => s.IsActive);
            if (activeSections > 0)
            {
                return (false, $"Cannot deactivate level. It has {activeSections} active section(s).");
            }

            // Validation: Check for active students
            var activeStudents = level.Students.Count(s => s.IsActive);
            if (activeStudents > 0)
            {
                return (false, $"Cannot deactivate level. It has {activeStudents} active student(s).");
            }

            level.IsActive = false;
            _unitOfWork.Levels.Update(level);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Level '{level.Name}' deactivated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error deactivating level: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ActivateLevelAsync(int id)
    {
        try
        {
            var level = await _unitOfWork.Levels
                .GetByIdAsync(id, "Department");

            if (level == null)
            {
                return (false, "Level not found.");
            }

            if (level.IsActive)
            {
                return (false, "Level is already active.");
            }

            // Validation: Check if parent department is active
            if (!level.Department.IsActive)
            {
                return (false, $"Cannot activate level. Parent department '{level.Department.Name}' is inactive.");
            }

            level.IsActive = true;
            _unitOfWork.Levels.Update(level);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Level '{level.Name}' activated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error activating level: {ex.Message}");
        }
    }

    public async Task<LevelStatistics> GetLevelStatisticsAsync(int id)
    {
        return await _unitOfWork.Levels.GetLevelStatisticsAsync(id);
    }
}
