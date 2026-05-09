using SmartClassRoom.Web.Models.Entities.Materials;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Material entity with domain-specific queries
/// </summary>
public interface IMaterialRepository : IRepository<Material>
{
    /// <summary>
    /// Get all materials for a course offering
    /// </summary>
    Task<IEnumerable<Material>> GetByCourseOfferingAsync(int courseOfferingId);

    /// <summary>
    /// Get materials in a specific folder
    /// </summary>
    Task<IEnumerable<Material>> GetByFolderAsync(int folderId);

    /// <summary>
    /// Get materials not in any folder (root level)
    /// </summary>
    Task<IEnumerable<Material>> GetRootMaterialsAsync(int courseOfferingId);

    /// <summary>
    /// Get material with course offering details
    /// </summary>
    Task<Material?> GetWithDetailsAsync(int materialId);

    /// <summary>
    /// Get total material count for a course offering
    /// </summary>
    Task<int> GetCountByCourseOfferingAsync(int courseOfferingId);

    /// <summary>
    /// Get materials by file type
    /// </summary>
    Task<IEnumerable<Material>> GetByFileTypeAsync(int courseOfferingId, string fileType);

    /// <summary>
    /// Get materials uploaded by a specific teacher
    /// </summary>
    Task<IEnumerable<Material>> GetByUploaderAsync(int teacherId);

    /// <summary>
    /// Increment download count for a material
    /// </summary>
    Task IncrementDownloadCountAsync(int materialId);

    /// <summary>
    /// Check if material exists and is active
    /// </summary>
    Task<bool> ExistsAndActiveAsync(int materialId);
}
