using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class MaterialsController : Controller
{
    private readonly IMaterialService _materialService;
    private readonly ILookupService _lookupService;
    private readonly ITermService _termService;

    public MaterialsController(
        IMaterialService materialService,
        ILookupService lookupService,
        ITermService termService)
    {
        _materialService = materialService;
        _lookupService = lookupService;
        _termService = termService;
    }

    // GET: Admin/Materials
    public async Task<IActionResult> Index(int? termId = null, int? departmentId = null)
    {
        var courseOfferings = await _materialService.GetCourseOfferingsWithMaterialsAsync(termId, departmentId);

        await PopulateFilterDropdowns(termId, departmentId);

        return View(courseOfferings);
    }

    // GET: Admin/Materials/Manage/5
    public async Task<IActionResult> Manage(int id)
    {
        var courseOffering = await _materialService.GetCourseOfferingMaterialsAsync(id);

        if (courseOffering == null)
        {
            TempData["ErrorMessage"] = "Course offering not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.AllowedExtensions = _materialService.GetAllowedExtensions();
        ViewBag.MaxFileSize = _materialService.GetMaxFileSize();
        ViewBag.MaxFileSizeMB = _materialService.GetMaxFileSize() / (1024 * 1024);

        return View(courseOffering);
    }

    // POST: Admin/Materials/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(104857600)] // 100 MB
    public async Task<IActionResult> Upload(int courseOfferingId, IFormFile file, string? title, string? description, int? folderId)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a file to upload.";
            return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            TempData["ErrorMessage"] = "Unable to identify current user.";
            return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
        }

        var result = await _materialService.UploadMaterialAsync(courseOfferingId, userId, file, title, description, folderId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
    }

    // POST: Admin/Materials/UploadMultiple
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(524288000)] // 500 MB for multiple files
    public async Task<IActionResult> UploadMultiple(int courseOfferingId, List<IFormFile> files, int? folderId)
    {
        if (files == null || files.Count == 0)
        {
            TempData["ErrorMessage"] = "Please select files to upload.";
            return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            TempData["ErrorMessage"] = "Unable to identify current user.";
            return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
        }

        var successCount = 0;
        var failedFiles = new List<string>();

        foreach (var file in files)
        {
            var result = await _materialService.UploadMaterialAsync(courseOfferingId, userId, file, null, null, folderId);
            if (result.Success)
            {
                successCount++;
            }
            else
            {
                failedFiles.Add($"{file.FileName}: {result.Message}");
            }
        }

        if (successCount > 0)
        {
            TempData["SuccessMessage"] = $"Successfully uploaded {successCount} file(s).";
        }

        if (failedFiles.Count > 0)
        {
            TempData["ErrorMessage"] = $"Failed to upload: {string.Join(", ", failedFiles)}";
        }

        return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
    }

    // POST: Admin/Materials/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int courseOfferingId)
    {
        var success = await _materialService.DeleteMaterialAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = "Material deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete material.";
        }

        return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
    }

    // POST: Admin/Materials/CreateFolder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFolder(int courseOfferingId, string name, int? parentFolderId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Folder name is required.";
            return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
        }

        var folder = await _materialService.CreateFolderAsync(courseOfferingId, name.Trim(), parentFolderId);

        if (folder != null)
        {
            TempData["SuccessMessage"] = $"Folder '{name}' created successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to create folder. A folder with this name may already exist.";
        }

        return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
    }

    // POST: Admin/Materials/RenameFolder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameFolder(int folderId, string newName, int courseOfferingId)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            TempData["ErrorMessage"] = "Folder name is required.";
            return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
        }

        var success = await _materialService.RenameFolderAsync(folderId, newName.Trim());

        if (success)
        {
            TempData["SuccessMessage"] = $"Folder renamed to '{newName}' successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to rename folder.";
        }

        return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
    }

    // POST: Admin/Materials/DeleteFolder/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFolder(int folderId, int courseOfferingId)
    {
        var (success, message) = await _materialService.DeleteFolderAsync(folderId);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
    }

    // POST: Admin/Materials/MoveToFolder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveToFolder(int materialId, int? folderId, int courseOfferingId)
    {
        var success = await _materialService.MoveMaterialToFolderAsync(materialId, folderId);

        if (success)
        {
            TempData["SuccessMessage"] = "Material moved successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to move material.";
        }

        return RedirectToAction(nameof(Manage), new { id = courseOfferingId });
    }

    // GET: Admin/Materials/Download/5
    public async Task<IActionResult> Download(int id)
    {
        var material = await _materialService.GetMaterialByIdAsync(id);

        if (material == null)
        {
            TempData["ErrorMessage"] = "Material not found.";
            return RedirectToAction(nameof(Index));
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", material.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(filePath))
        {
            TempData["ErrorMessage"] = "File not found on server.";
            return RedirectToAction(nameof(Manage), new { id = material.CourseOfferingId });
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var extension = Path.GetExtension(material.FilePath).ToLowerInvariant();
        var contentType = GetContentType(extension);
        var originalFileName = $"{material.Title}{extension}";
        return File(fileBytes, contentType, originalFileName);
    }

    #region API Endpoints for AJAX

    // GET: Admin/Materials/GetFolders/5
    [HttpGet]
    public async Task<IActionResult> GetFolders(int courseOfferingId)
    {
        var courseOffering = await _materialService.GetCourseOfferingMaterialsAsync(courseOfferingId);

        if (courseOffering == null)
        {
            return NotFound();
        }

        return Json(courseOffering.Folders);
    }

    // GET: Admin/Materials/GetMaterials/5
    [HttpGet]
    public async Task<IActionResult> GetMaterials(int courseOfferingId, int? folderId = null)
    {
        var courseOffering = await _materialService.GetCourseOfferingMaterialsAsync(courseOfferingId);

        if (courseOffering == null)
        {
            return NotFound();
        }

        if (folderId == null)
        {
            return Json(courseOffering.RootMaterials);
        }

        var folder = FindFolder(courseOffering.Folders, folderId.Value);
        return Json(folder?.Materials ?? new List<MaterialViewModel>());
    }

    private MaterialFolderViewModel? FindFolder(List<MaterialFolderViewModel> folders, int folderId)
    {
        foreach (var folder in folders)
        {
            if (folder.Id == folderId)
                return folder;

            var found = FindFolder(folder.SubFolders, folderId);
            if (found != null)
                return found;
        }
        return null;
    }

    #endregion

    #region Helper Methods

    private async Task PopulateFilterDropdowns(int? selectedTermId, int? selectedDepartmentId)
    {
        // Get terms for filter
        var termsResult = await _termService.GetTermsPagedAsync(pageNumber: 1, pageSize: 100);
        var terms = termsResult.Items.OrderByDescending(t => t.StartDate).ToList();
        ViewBag.Terms = new SelectList(terms, "Id", "Name", selectedTermId);

        // Get departments for filter
        var departments = await _lookupService.GetAllDepartmentsAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedDepartmentId);

        ViewBag.SelectedTermId = selectedTermId;
        ViewBag.SelectedDepartmentId = selectedDepartmentId;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    #endregion
}
