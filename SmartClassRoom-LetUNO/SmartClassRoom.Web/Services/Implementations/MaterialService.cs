using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Materials;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service implementation for Material management
/// </summary>
public class MaterialService : IMaterialService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;
    private readonly string _materialsBasePath;

    // Allowed file types and their MIME types
    private static readonly Dictionary<string, string> AllowedFileTypes = new()
    {
        { ".pdf", "application/pdf" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".doc", "application/msword" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".xls", "application/vnd.ms-excel" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".zip", "application/zip" },
        { ".rar", "application/x-rar-compressed" },
        { ".mp4", "video/mp4" },
        { ".mp3", "audio/mpeg" },
        { ".txt", "text/plain" }
    };

    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

    public MaterialService(IUnitOfWork unitOfWork, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
        _materialsBasePath = configuration["MaterialsStorage:BasePath"] ?? @"C:\Smart Class Room Materials";
    }

    // ==================== Admin Dashboard Methods ====================

    public async Task<IEnumerable<CourseOfferingMaterialsViewModel>> GetCourseOfferingsWithMaterialsAsync(int? termId = null, int? departmentId = null)
    {
        var offerings = await _unitOfWork.CourseOfferings.GetAllAsync("Course", "Teacher", "Term", "Section");

        if (termId.HasValue)
        {
            offerings = offerings.Where(o => o.TermId == termId.Value);
        }

        if (departmentId.HasValue)
        {
            offerings = offerings.Where(o => o.Course.DepartmentId == departmentId.Value);
        }

        var result = new List<CourseOfferingMaterialsViewModel>();

        foreach (var offering in offerings.Where(o => o.Status == "Active"))
        {
            var materialCount = await _unitOfWork.Materials.GetCountByCourseOfferingAsync(offering.Id);
            var materials = await _unitOfWork.Materials.GetByCourseOfferingAsync(offering.Id);
            var totalSize = materials.Sum(m => m.FileSize);
            var lastUpload = materials.OrderByDescending(m => m.UploadedAt).FirstOrDefault()?.UploadedAt;

            result.Add(new CourseOfferingMaterialsViewModel
            {
                CourseOfferingId = offering.Id,
                CourseCode = offering.Course.Code,
                CourseName = offering.Course.Name,
                TeacherName = offering.Teacher?.FullName ?? "TBA",
                TermName = offering.Term.Name,
                SectionName = offering.Section?.Name,
                TotalMaterials = materialCount,
                TotalSize = totalSize,
                FormattedTotalSize = FormatFileSize(totalSize),
                LastUploadDate = lastUpload
            });
        }

        return result.OrderBy(o => o.CourseName);
    }

    public async Task<CourseOfferingMaterialsViewModel?> GetCourseOfferingMaterialsAsync(int courseOfferingId)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null) return null;

        var folders = await _unitOfWork.MaterialFolders.GetRootFoldersAsync(courseOfferingId);
        var rootMaterials = await _unitOfWork.Materials.GetRootMaterialsAsync(courseOfferingId);
        var allMaterials = await _unitOfWork.Materials.GetByCourseOfferingAsync(courseOfferingId);

        var viewModel = new CourseOfferingMaterialsViewModel
        {
            CourseOfferingId = offering.Id,
            CourseCode = offering.Course.Code,
            CourseName = offering.Course.Name,
            TeacherName = offering.Teacher?.FullName ?? "TBA",
            TermName = offering.Term.Name,
            SectionName = offering.Section?.Name,
            TotalMaterials = allMaterials.Count(),
            TotalSize = allMaterials.Sum(m => m.FileSize),
            FormattedTotalSize = FormatFileSize(allMaterials.Sum(m => m.FileSize)),
            LastUploadDate = allMaterials.OrderByDescending(m => m.UploadedAt).FirstOrDefault()?.UploadedAt,
            Folders = await MapFoldersToViewModelsAsync(folders),
            RootMaterials = rootMaterials.Select(MapMaterialToViewModel).ToList()
        };

        return viewModel;
    }

    public async Task<MaterialUploadResult> UploadMaterialAsync(int courseOfferingId, int uploadedById, IFormFile file, string? title, string? description, int? folderId)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return new MaterialUploadResult
            {
                Success = false,
                Message = "No file provided.",
                ErrorCode = "NO_FILE"
            };
        }

        // Check file size
        if (file.Length > MaxFileSize)
        {
            return new MaterialUploadResult
            {
                Success = false,
                Message = $"File size exceeds maximum allowed ({FormatFileSize(MaxFileSize)}).",
                ErrorCode = "FILE_TOO_LARGE"
            };
        }

        // Check file type
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedFileTypes.ContainsKey(extension))
        {
            return new MaterialUploadResult
            {
                Success = false,
                Message = $"File type '{extension}' is not allowed.",
                ErrorCode = "INVALID_FILE_TYPE"
            };
        }

        // Verify course offering exists
        var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(courseOfferingId);
        if (offering == null)
        {
            return new MaterialUploadResult
            {
                Success = false,
                Message = "Course offering not found.",
                ErrorCode = "OFFERING_NOT_FOUND"
            };
        }

        // Verify folder exists if provided
        if (folderId.HasValue)
        {
            var folder = await _unitOfWork.MaterialFolders.GetByIdAsync(folderId.Value);
            if (folder == null || folder.CourseOfferingId != courseOfferingId)
            {
                return new MaterialUploadResult
                {
                    Success = false,
                    Message = "Folder not found or does not belong to this course.",
                    ErrorCode = "FOLDER_NOT_FOUND"
                };
            }
        }

        try
        {
            // Get course offering with course details for folder name
            var offeringWithCourse = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
            var courseName = SanitizeFolderName(offeringWithCourse?.Course?.Name ?? $"Course_{courseOfferingId}");

            // Build folder path hierarchy
            var folderPath = await BuildFolderPathAsync(folderId);

            // Create upload directory: {BasePath}\{CourseName}\{FolderHierarchy}\
            var uploadPath = Path.Combine(_materialsBasePath, courseName);
            if (!string.IsNullOrEmpty(folderPath))
            {
                uploadPath = Path.Combine(uploadPath, folderPath);
            }

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Use the material title (from dashboard) as filename, with extension from original file
            var materialTitle = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(file.FileName) : title;
            var safeTitle = SanitizeFileName(materialTitle);
            var fileName = $"{safeTitle}{extension}";

            // Check if file with same name exists, add timestamp if so
            var filePath = Path.Combine(uploadPath, fileName);
            if (File.Exists(filePath))
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                fileName = $"{safeTitle}_{timestamp}{extension}";
                filePath = Path.Combine(uploadPath, fileName);
            }

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Build relative path for database storage (relative to base materials path)
            var relativePath = Path.Combine(courseName, folderPath ?? "", fileName);

            // Create material record
            var material = new Material
            {
                Title = materialTitle,
                Description = description,
                FilePath = relativePath, // Store relative path from Materials base folder
                FileType = extension.TrimStart('.').ToUpper(),
                FileSize = file.Length,
                CourseOfferingId = courseOfferingId,
                FolderId = folderId,
                UploadedById = uploadedById,
                UploadedAt = DateTime.UtcNow,
                DownloadCount = 0,
                IsActive = true
            };

            await _unitOfWork.Materials.AddAsync(material);
            await _unitOfWork.SaveChangesAsync();

            return new MaterialUploadResult
            {
                Success = true,
                Message = "File uploaded successfully.",
                Material = material
            };
        }
        catch (Exception ex)
        {
            return new MaterialUploadResult
            {
                Success = false,
                Message = $"Error uploading file: {ex.Message}",
                ErrorCode = "UPLOAD_ERROR"
            };
        }
    }

    public async Task<bool> DeleteMaterialAsync(int materialId)
    {
        var material = await _unitOfWork.Materials.GetByIdAsync(materialId);
        if (material == null) return false;

        try
        {
            // Delete physical file from materials storage location
            var physicalPath = Path.Combine(_materialsBasePath, material.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            // Soft delete in database
            material.IsActive = false;
            _unitOfWork.Materials.Update(material);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<MaterialFolder?> CreateFolderAsync(int courseOfferingId, string name, int? parentFolderId)
    {
        // Check if name already exists at this level
        if (await _unitOfWork.MaterialFolders.NameExistsAsync(courseOfferingId, name, parentFolderId))
        {
            return null;
        }

        var folder = new MaterialFolder
        {
            Name = name,
            CourseOfferingId = courseOfferingId,
            ParentFolderId = parentFolderId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MaterialFolders.AddAsync(folder);
        await _unitOfWork.SaveChangesAsync();

        return folder;
    }

    public async Task<bool> RenameFolderAsync(int folderId, string newName)
    {
        var folder = await _unitOfWork.MaterialFolders.GetByIdAsync(folderId);
        if (folder == null) return false;

        // Check if new name already exists
        if (await _unitOfWork.MaterialFolders.NameExistsAsync(folder.CourseOfferingId, newName, folder.ParentFolderId, folderId))
        {
            return false;
        }

        folder.Name = newName;
        _unitOfWork.MaterialFolders.Update(folder);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<(bool Success, string Message)> DeleteFolderAsync(int folderId)
    {
        var folder = await _unitOfWork.MaterialFolders.GetByIdAsync(folderId);
        if (folder == null)
        {
            return (false, "Folder not found.");
        }

        // Check if folder has content
        if (await _unitOfWork.MaterialFolders.HasContentAsync(folderId))
        {
            return (false, "Folder is not empty. Move or delete its contents first.");
        }

        _unitOfWork.MaterialFolders.Delete(folder);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Folder deleted successfully.");
    }

    public async Task<bool> MoveMaterialToFolderAsync(int materialId, int? folderId)
    {
        var material = await _unitOfWork.Materials.GetByIdAsync(materialId);
        if (material == null) return false;

        // Verify folder exists and belongs to same course offering
        if (folderId.HasValue)
        {
            var folder = await _unitOfWork.MaterialFolders.GetByIdAsync(folderId.Value);
            if (folder == null || folder.CourseOfferingId != material.CourseOfferingId)
            {
                return false;
            }
        }

        material.FolderId = folderId;
        _unitOfWork.Materials.Update(material);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Material?> GetMaterialByIdAsync(int materialId)
    {
        return await _unitOfWork.Materials.GetWithDetailsAsync(materialId);
    }

    // ==================== Mobile API Methods ====================

    public async Task<IEnumerable<CourseOfferingMaterialsDto>> GetEnrolledCoursesWithMaterialsAsync(int studentId)
    {
        var enrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(studentId);
        var result = new List<CourseOfferingMaterialsDto>();

        foreach (var enrollment in enrollments.Where(e => e.Status == "Enrolled"))
        {
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);
            if (offering == null) continue;

            var materialCount = await _unitOfWork.Materials.GetCountByCourseOfferingAsync(offering.Id);

            result.Add(new CourseOfferingMaterialsDto
            {
                CourseOfferingId = offering.Id,
                CourseCode = offering.Course.Code,
                CourseName = offering.Course.Name,
                TeacherName = offering.Teacher?.FullName ?? "TBA",
                TermName = offering.Term.Name,
                TotalMaterials = materialCount
            });
        }

        return result.OrderBy(c => c.CourseName);
    }

    public async Task<CourseOfferingMaterialsDto?> GetMaterialsForCourseAsync(int studentId, int courseOfferingId)
    {
        // Check enrollment
        if (!await CanAccessCourseAsync(studentId, courseOfferingId))
        {
            return null;
        }

        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null) return null;

        var folders = await _unitOfWork.MaterialFolders.GetRootFoldersAsync(courseOfferingId);
        var rootMaterials = await _unitOfWork.Materials.GetRootMaterialsAsync(courseOfferingId);

        return new CourseOfferingMaterialsDto
        {
            CourseOfferingId = offering.Id,
            CourseCode = offering.Course.Code,
            CourseName = offering.Course.Name,
            TeacherName = offering.Teacher?.FullName ?? "TBA",
            TermName = offering.Term.Name,
            TotalMaterials = await _unitOfWork.Materials.GetCountByCourseOfferingAsync(courseOfferingId),
            Folders = await MapFoldersToDtosAsync(folders),
            RootMaterials = rootMaterials.Select(MapMaterialToDto).ToList()
        };
    }

    public async Task<FolderMaterialsDto?> GetMaterialsInFolderAsync(int studentId, int folderId)
    {
        var folder = await _unitOfWork.MaterialFolders.GetWithMaterialsAsync(folderId);
        if (folder == null) return null;

        // Check enrollment
        if (!await CanAccessCourseAsync(studentId, folder.CourseOfferingId))
        {
            return null;
        }

        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(folder.CourseOfferingId);

        return new FolderMaterialsDto
        {
            FolderId = folder.Id,
            FolderName = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            CourseOfferingId = folder.CourseOfferingId,
            CourseName = offering?.Course?.Name ?? "Unknown",
            SubFolders = folder.SubFolders.Select(sf => new MaterialFolderDto
            {
                Id = sf.Id,
                Name = sf.Name,
                ParentFolderId = sf.ParentFolderId,
                MaterialCount = sf.Materials.Count(m => m.IsActive)
            }).ToList(),
            Materials = folder.Materials.Where(m => m.IsActive).Select(MapMaterialToDto).ToList()
        };
    }

    public async Task<MaterialDownloadResult?> DownloadMaterialAsync(int studentId, int materialId)
    {
        var material = await _unitOfWork.Materials.GetWithDetailsAsync(materialId);
        if (material == null || !material.IsActive) return null;

        // Check enrollment
        if (!await CanAccessCourseAsync(studentId, material.CourseOfferingId))
        {
            return null;
        }

        // Get file from materials storage location
        var physicalPath = Path.Combine(_materialsBasePath, material.FilePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(physicalPath)) return null;

        // Increment download count
        await _unitOfWork.Materials.IncrementDownloadCountAsync(materialId);
        await _unitOfWork.SaveChangesAsync();

        var fileBytes = await File.ReadAllBytesAsync(physicalPath);
        var extension = Path.GetExtension(material.FilePath).ToLowerInvariant();
        var contentType = AllowedFileTypes.GetValueOrDefault(extension, "application/octet-stream");

        return new MaterialDownloadResult
        {
            FileBytes = fileBytes,
            ContentType = contentType,
            FileName = $"{material.Title}{extension}"
        };
    }

    // ==================== Utility Methods ====================

    public async Task<bool> CanAccessCourseAsync(int userId, int courseOfferingId)
    {
        return await _unitOfWork.StudentEnrollments.IsStudentEnrolledAsync(userId, courseOfferingId);
    }

    public IEnumerable<string> GetAllowedExtensions()
    {
        return AllowedFileTypes.Keys;
    }

    public long GetMaxFileSize()
    {
        return MaxFileSize;
    }

    // ==================== Private Helper Methods ====================

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Replace(" ", "_");
    }

    private static string SanitizeFolderName(string folderName)
    {
        var invalidChars = Path.GetInvalidPathChars();
        var sanitized = string.Join("_", folderName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        // Also remove characters that are invalid in folder names
        sanitized = sanitized.Replace(":", "_").Replace("*", "_").Replace("?", "_")
                            .Replace("\"", "_").Replace("<", "_").Replace(">", "_").Replace("|", "_");
        return sanitized.Trim();
    }

    /// <summary>
    /// Builds the folder path hierarchy from root to the given folder
    /// </summary>
    private async Task<string?> BuildFolderPathAsync(int? folderId)
    {
        if (!folderId.HasValue) return null;

        var pathParts = new List<string>();
        var currentFolderId = folderId;

        // Walk up the folder hierarchy to build the full path
        while (currentFolderId.HasValue)
        {
            var folder = await _unitOfWork.MaterialFolders.GetByIdAsync(currentFolderId.Value);
            if (folder == null) break;

            pathParts.Insert(0, SanitizeFolderName(folder.Name));
            currentFolderId = folder.ParentFolderId;
        }

        return pathParts.Count > 0 ? Path.Combine(pathParts.ToArray()) : null;
    }

    private static string GetFileTypeIcon(string fileType)
    {
        return fileType.ToUpper() switch
        {
            "PDF" => "bi-file-earmark-pdf",
            "DOC" or "DOCX" => "bi-file-earmark-word",
            "PPT" or "PPTX" => "bi-file-earmark-ppt",
            "XLS" or "XLSX" => "bi-file-earmark-excel",
            "JPG" or "JPEG" or "PNG" or "GIF" => "bi-file-earmark-image",
            "ZIP" or "RAR" => "bi-file-earmark-zip",
            "MP4" => "bi-file-earmark-play",
            "MP3" => "bi-file-earmark-music",
            "TXT" => "bi-file-earmark-text",
            _ => "bi-file-earmark"
        };
    }

    private MaterialViewModel MapMaterialToViewModel(Material material)
    {
        return new MaterialViewModel
        {
            Id = material.Id,
            Title = material.Title,
            Description = material.Description,
            FileType = material.FileType ?? "Unknown",
            FileTypeIcon = GetFileTypeIcon(material.FileType ?? ""),
            FileSize = material.FileSize,
            FormattedFileSize = FormatFileSize(material.FileSize),
            UploadedAt = material.UploadedAt,
            UploadedByName = material.UploadedBy?.FullName ?? "Unknown",
            FolderId = material.FolderId,
            FolderName = material.Folder?.Name,
            DownloadCount = material.DownloadCount ?? 0
        };
    }

    private MaterialDto MapMaterialToDto(Material material)
    {
        return new MaterialDto
        {
            Id = material.Id,
            Title = material.Title,
            Description = material.Description,
            FileType = material.FileType ?? "Unknown",
            FileSize = material.FileSize,
            FormattedFileSize = FormatFileSize(material.FileSize),
            UploadedAt = material.UploadedAt,
            UploadedByName = material.UploadedBy?.FullName ?? "Unknown",
            FolderId = material.FolderId
        };
    }

    private async Task<List<MaterialFolderViewModel>> MapFoldersToViewModelsAsync(IEnumerable<MaterialFolder> folders)
    {
        var result = new List<MaterialFolderViewModel>();

        foreach (var folder in folders)
        {
            var folderVm = new MaterialFolderViewModel
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentFolderId = folder.ParentFolderId,
                MaterialCount = folder.Materials.Count(m => m.IsActive),
                Materials = folder.Materials.Where(m => m.IsActive).Select(MapMaterialToViewModel).ToList()
            };

            // Get subfolders
            var subFolders = await _unitOfWork.MaterialFolders.GetSubFoldersAsync(folder.Id);
            folderVm.SubFolders = await MapFoldersToViewModelsAsync(subFolders);

            result.Add(folderVm);
        }

        return result;
    }

    private async Task<List<MaterialFolderDto>> MapFoldersToDtosAsync(IEnumerable<MaterialFolder> folders)
    {
        var result = new List<MaterialFolderDto>();

        foreach (var folder in folders)
        {
            var folderDto = new MaterialFolderDto
            {
                Id = folder.Id,
                Name = folder.Name,
                ParentFolderId = folder.ParentFolderId,
                MaterialCount = folder.Materials.Count(m => m.IsActive)
            };

            // Get subfolders
            var subFolders = await _unitOfWork.MaterialFolders.GetSubFoldersAsync(folder.Id);
            folderDto.SubFolders = await MapFoldersToDtosAsync(subFolders);

            result.Add(folderDto);
        }

        return result;
    }
}
