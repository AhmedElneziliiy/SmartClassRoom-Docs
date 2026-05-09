using Microsoft.AspNetCore.Http;
using SmartClassRoom.Web.Models.Entities.Materials;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for Material management operations
/// </summary>
public interface IMaterialService
{
    // ==================== Admin Dashboard Methods ====================

    /// <summary>
    /// Get all course offerings with their material counts
    /// </summary>
    Task<IEnumerable<CourseOfferingMaterialsViewModel>> GetCourseOfferingsWithMaterialsAsync(int? termId = null, int? departmentId = null);

    /// <summary>
    /// Get materials and folders for a specific course offering
    /// </summary>
    Task<CourseOfferingMaterialsViewModel?> GetCourseOfferingMaterialsAsync(int courseOfferingId);

    /// <summary>
    /// Upload a material file
    /// </summary>
    Task<MaterialUploadResult> UploadMaterialAsync(int courseOfferingId, int uploadedById, IFormFile file, string? title, string? description, int? folderId);

    /// <summary>
    /// Delete a material (soft delete and remove file)
    /// </summary>
    Task<bool> DeleteMaterialAsync(int materialId);

    /// <summary>
    /// Create a new folder
    /// </summary>
    Task<MaterialFolder?> CreateFolderAsync(int courseOfferingId, string name, int? parentFolderId);

    /// <summary>
    /// Rename a folder
    /// </summary>
    Task<bool> RenameFolderAsync(int folderId, string newName);

    /// <summary>
    /// Delete a folder (must be empty)
    /// </summary>
    Task<(bool Success, string Message)> DeleteFolderAsync(int folderId);

    /// <summary>
    /// Move a material to a different folder
    /// </summary>
    Task<bool> MoveMaterialToFolderAsync(int materialId, int? folderId);

    /// <summary>
    /// Get material by ID with details
    /// </summary>
    Task<Material?> GetMaterialByIdAsync(int materialId);

    // ==================== Mobile API Methods ====================

    /// <summary>
    /// Get enrolled courses with material counts for a student
    /// </summary>
    Task<IEnumerable<CourseOfferingMaterialsDto>> GetEnrolledCoursesWithMaterialsAsync(int studentId);

    /// <summary>
    /// Get materials for a course (with enrollment check)
    /// </summary>
    Task<CourseOfferingMaterialsDto?> GetMaterialsForCourseAsync(int studentId, int courseOfferingId);

    /// <summary>
    /// Get materials in a specific folder (with enrollment check)
    /// </summary>
    Task<FolderMaterialsDto?> GetMaterialsInFolderAsync(int studentId, int folderId);

    /// <summary>
    /// Download a material file (with enrollment check)
    /// </summary>
    Task<MaterialDownloadResult?> DownloadMaterialAsync(int studentId, int materialId);

    // ==================== Utility Methods ====================

    /// <summary>
    /// Check if user can access course materials
    /// </summary>
    Task<bool> CanAccessCourseAsync(int userId, int courseOfferingId);

    /// <summary>
    /// Get allowed file extensions
    /// </summary>
    IEnumerable<string> GetAllowedExtensions();

    /// <summary>
    /// Get max file size in bytes
    /// </summary>
    long GetMaxFileSize();
}

// ==================== ViewModels and DTOs ====================

/// <summary>
/// ViewModel for displaying course offerings with material info
/// </summary>
public class CourseOfferingMaterialsViewModel
{
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string? SectionName { get; set; }
    public int TotalMaterials { get; set; }
    public long TotalSize { get; set; }
    public string FormattedTotalSize { get; set; } = string.Empty;
    public DateTime? LastUploadDate { get; set; }
    public List<MaterialFolderViewModel> Folders { get; set; } = new();
    public List<MaterialViewModel> RootMaterials { get; set; } = new();
}

/// <summary>
/// ViewModel for displaying a folder
/// </summary>
public class MaterialFolderViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public int MaterialCount { get; set; }
    public List<MaterialFolderViewModel> SubFolders { get; set; } = new();
    public List<MaterialViewModel> Materials { get; set; } = new();
}

/// <summary>
/// ViewModel for displaying a material
/// </summary>
public class MaterialViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string FileTypeIcon { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FormattedFileSize { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public int? FolderId { get; set; }
    public string? FolderName { get; set; }
    public int DownloadCount { get; set; }
}

/// <summary>
/// DTO for mobile API - course materials
/// </summary>
public class CourseOfferingMaterialsDto
{
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public int TotalMaterials { get; set; }
    public List<MaterialFolderDto> Folders { get; set; } = new();
    public List<MaterialDto> RootMaterials { get; set; } = new();
}

/// <summary>
/// DTO for mobile API - folder
/// </summary>
public class MaterialFolderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public int MaterialCount { get; set; }
    public List<MaterialFolderDto> SubFolders { get; set; } = new();
}

/// <summary>
/// DTO for mobile API - material
/// </summary>
public class MaterialDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FormattedFileSize { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public int? FolderId { get; set; }
}

/// <summary>
/// DTO for mobile API - folder contents
/// </summary>
public class FolderMaterialsDto
{
    public int FolderId { get; set; }
    public string FolderName { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public int CourseOfferingId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public List<MaterialFolderDto> SubFolders { get; set; } = new();
    public List<MaterialDto> Materials { get; set; } = new();
}

/// <summary>
/// Result of material upload operation
/// </summary>
public class MaterialUploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Material? Material { get; set; }
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Result of material download operation
/// </summary>
public class MaterialDownloadResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
