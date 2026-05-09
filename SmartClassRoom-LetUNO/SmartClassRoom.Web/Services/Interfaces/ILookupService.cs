using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service for managing lookup data (departments, levels, sections, groups, universities)
/// </summary>
public interface ILookupService
{
    /// <summary>
    /// Get all departments ordered by name
    /// </summary>
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();

    /// <summary>
    /// Get all levels ordered by name
    /// </summary>
    Task<IEnumerable<Level>> GetAllLevelsAsync();

    /// <summary>
    /// Get all sections ordered by name
    /// </summary>
    Task<IEnumerable<Section>> GetAllSectionsAsync();

    /// <summary>
    /// Get all groups ordered by name
    /// </summary>
    Task<IEnumerable<Group>> GetAllGroupsAsync();

    /// <summary>
    /// Get all universities ordered by name
    /// </summary>
    Task<IEnumerable<University>> GetAllUniversitiesAsync();

    /// <summary>
    /// Get levels by department for cascading dropdowns
    /// </summary>
    Task<IEnumerable<Level>> GetLevelsByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get sections by level for cascading dropdowns
    /// </summary>
    Task<IEnumerable<Section>> GetSectionsByLevelAsync(int levelId);
}
