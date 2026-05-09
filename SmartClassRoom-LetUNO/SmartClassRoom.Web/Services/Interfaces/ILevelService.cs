using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Levels;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for managing Level entities
/// </summary>
public interface ILevelService
{
    /// <summary>
    /// Get paginated list of levels with optional filtering and sorting
    /// </summary>
    Task<PagedResult<Level>> GetLevelsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Get level by ID
    /// </summary>
    Task<Level?> GetLevelByIdAsync(int id);

    /// <summary>
    /// Get level with all related details loaded
    /// </summary>
    Task<Level?> GetLevelWithDetailsAsync(int id);

    /// <summary>
    /// Create a new level
    /// </summary>
    Task<(bool Success, string Message)> CreateLevelAsync(CreateLevelRequest request);

    /// <summary>
    /// Update an existing level
    /// </summary>
    Task<(bool Success, string Message)> UpdateLevelAsync(EditLevelRequest request);

    /// <summary>
    /// Deactivate a level (soft delete)
    /// </summary>
    Task<(bool Success, string Message)> DeactivateLevelAsync(int id);

    /// <summary>
    /// Activate a level
    /// </summary>
    Task<(bool Success, string Message)> ActivateLevelAsync(int id);

    /// <summary>
    /// Get statistics for a specific level
    /// </summary>
    Task<LevelStatistics> GetLevelStatisticsAsync(int id);
}
