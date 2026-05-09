using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

/// <summary>
/// API controller for mobile access to course materials
/// Students can view and download materials for their enrolled courses
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MaterialsController : ControllerBase
{
    private readonly IMaterialService _materialService;

    public MaterialsController(IMaterialService materialService)
    {
        _materialService = materialService;
    }

    /// <summary>
    /// Get all enrolled courses with material counts
    /// GET: api/Materials/courses
    /// </summary>
    [HttpGet("courses")]
    public async Task<ActionResult> GetEnrolledCoursesWithMaterials()
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var courses = await _materialService.GetEnrolledCoursesWithMaterialsAsync(studentId);
            return Ok(courses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get materials for a specific course offering
    /// GET: api/Materials/course/{courseOfferingId}
    /// </summary>
    [HttpGet("course/{courseOfferingId}")]
    public async Task<ActionResult> GetMaterialsForCourse(int courseOfferingId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var materials = await _materialService.GetMaterialsForCourseAsync(studentId, courseOfferingId);

            if (materials == null)
                return Forbid("You are not enrolled in this course");

            return Ok(materials);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get materials in a specific folder
    /// GET: api/Materials/folder/{folderId}
    /// </summary>
    [HttpGet("folder/{folderId}")]
    public async Task<ActionResult> GetMaterialsInFolder(int folderId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var folderMaterials = await _materialService.GetMaterialsInFolderAsync(studentId, folderId);

            if (folderMaterials == null)
                return Forbid("You do not have access to this folder");

            return Ok(folderMaterials);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Download a material file
    /// GET: api/Materials/download/{materialId}
    /// </summary>
    [HttpGet("download/{materialId}")]
    public async Task<ActionResult> DownloadMaterial(int materialId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var result = await _materialService.DownloadMaterialAsync(studentId, materialId);

            if (result == null)
                return Forbid("You do not have access to this material or material not found");

            return File(result.FileBytes, result.ContentType, result.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "Material file not found on server" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get material details (without downloading)
    /// GET: api/Materials/{materialId}
    /// </summary>
    [HttpGet("{materialId}")]
    public async Task<ActionResult> GetMaterialDetails(int materialId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var material = await _materialService.GetMaterialByIdAsync(materialId);

            if (material == null)
                return NotFound(new { message = "Material not found" });

            // Check if student can access this material
            var canAccess = await _materialService.CanAccessCourseAsync(studentId, material.CourseOfferingId);
            if (!canAccess)
                return Forbid("You do not have access to this material");

            return Ok(new
            {
                material.Id,
                material.Title,
                material.Description,
                material.FileType,
                material.FileSize,
                FormattedFileSize = FormatBytes(material.FileSize),
                material.UploadedAt,
                UploadedByName = material.UploadedBy?.FullName ?? "Unknown",
                material.FolderId,
                material.DownloadCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get allowed file extensions and max file size (useful for mobile app info)
    /// GET: api/Materials/config
    /// </summary>
    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult GetMaterialsConfig()
    {
        return Ok(new
        {
            AllowedExtensions = _materialService.GetAllowedExtensions(),
            MaxFileSizeBytes = _materialService.GetMaxFileSize(),
            MaxFileSizeMB = _materialService.GetMaxFileSize() / (1024 * 1024)
        });
    }

    private int GetCurrentStudentId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 Bytes";

        string[] sizes = { "Bytes", "KB", "MB", "GB" };
        int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024));
        return $"{Math.Round(bytes / Math.Pow(1024, i), 2)} {sizes[i]}";
    }
}
