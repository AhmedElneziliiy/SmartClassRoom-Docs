using SmartClassRoom.Web.Models.Entities.Materials;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for MaterialFolder entity with domain-specific queries
/// </summary>
public interface IMaterialFolderRepository : IRepository<MaterialFolder>
{
    /// <summary>
    /// Get all folders for a course offering
    /// </summary>
    Task<IEnumerable<MaterialFolder>> GetByCourseOfferingAsync(int courseOfferingId);

    /// <summary>
    /// Get root folders (no parent) for a course offering
    /// </summary>
    Task<IEnumerable<MaterialFolder>> GetRootFoldersAsync(int courseOfferingId);

    /// <summary>
    /// Get subfolders of a folder
    /// </summary>
    Task<IEnumerable<MaterialFolder>> GetSubFoldersAsync(int parentFolderId);

    /// <summary>
    /// Get folder with its materials
    /// </summary>
    Task<MaterialFolder?> GetWithMaterialsAsync(int folderId);

    /// <summary>
    /// Get folder with all nested content (recursive)
    /// </summary>
    Task<MaterialFolder?> GetWithNestedContentAsync(int folderId);

    /// <summary>
    /// Check if folder has any materials or subfolders
    /// </summary>
    Task<bool> HasContentAsync(int folderId);

    /// <summary>
    /// Get material count in folder (including subfolders)
    /// </summary>
    Task<int> GetMaterialCountAsync(int folderId, bool includeSubfolders = false);

    /// <summary>
    /// Check if folder name exists in course offering (at same level)
    /// </summary>
    Task<bool> NameExistsAsync(int courseOfferingId, string name, int? parentFolderId, int? excludeFolderId = null);
}
