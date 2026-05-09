using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Level entity with domain-specific queries
/// </summary>
public interface ILevelRepository : IRepository<Level>
{
    /// <summary>
    /// Get all levels for a specific department
    /// </summary>
    Task<IEnumerable<Level>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get all active levels for a specific department
    /// </summary>
    Task<IEnumerable<Level>> GetActiveLevelsByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get level with all related details loaded
    /// </summary>
    Task<Level?> GetLevelWithDetailsAsync(int levelId);

    /// <summary>
    /// Get statistics for a specific level
    /// </summary>
    Task<LevelStatistics> GetLevelStatisticsAsync(int levelId);
}
