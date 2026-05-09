using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Models.ViewModels.Users;
using SmartClassRoom.Web.Models.ViewModels.Universities;
using SmartClassRoom.Web.Models.ViewModels.Departments;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ILookupService _lookupService;
    private readonly IFaceEnrollmentService _faceEnrollmentService;

    public UsersController(
        IUserService userService,
        ILookupService lookupService,
        IFaceEnrollmentService faceEnrollmentService)
    {
        _userService = userService;
        _lookupService = lookupService;
        _faceEnrollmentService = faceEnrollmentService;
    }

    // GET: Admin/Users
    public async Task<IActionResult> Index(
        int page = 1,
        string? role = null,
        string? userType = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Fixed page size
        const int pageSize = 20;

        // Get paginated users
        var pagedUsers = await _userService.GetUsersPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            role: role,
            userType: userType,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        // Get all users for statistics (unfiltered)
        var allUsers = await _userService.GetAllUsersAsync();

        // Calculate statistics
        ViewBag.TotalUsers = allUsers.Count();
        ViewBag.TotalStudents = allUsers.Count(u => u.UserType == "Student");
        ViewBag.TotalTeachers = allUsers.Count(u => u.UserType == "Teacher");
        ViewBag.ActiveUsers = allUsers.Count(u => u.IsActive);
        ViewBag.InactiveUsers = allUsers.Count(u => !u.IsActive);
        ViewBag.StudentsActive = allUsers.Count(u => u.UserType == "Student" && u.IsActive);
        ViewBag.TeachersActive = allUsers.Count(u => u.UserType == "Teacher" && u.IsActive);

        // Create pagination view model with preserved route values
        var paginationViewModel = PaginationViewModel.Create(
            pagedUsers,
            routeValues: new Dictionary<string, string?>
            {
                { "role", role },
                { "userType", userType },
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Users",
            areaName: "Admin");

        // Pass pagination view model to view
        ViewBag.Pagination = paginationViewModel;

        // Pass filter values to view
        ViewBag.SelectedRole = role;
        ViewBag.SelectedUserType = userType;
        ViewBag.SelectedIsActive = isActive;

        // Pass sorting values to view
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        // Get all roles for filter dropdown
        ViewBag.Roles = await _userService.GetAllRolesAsync();

        return View(pagedUsers.Items);
    }

    // GET: Admin/Users/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();

        // Initialize model with default values
        var model = new CreateUserRequest
        {
            UserType = "Student",
            Role = "Student",
            IsActive = true
        };

        return View(model);
    }

    // POST: Admin/Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserRequest model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _userService.CreateUserAsync(model);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"User {model.FullName} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Users/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userService.GetUserRolesAsync(id);

        var model = new EditUserRequest
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            NationalId = user.NationalId,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Address = user.Address,
            UserType = user.UserType,
            Role = roles.FirstOrDefault() ?? "Student",
            IsActive = user.IsActive,
            UniversityId = user.UniversityId
        };

        // Load type-specific properties
        if (user.UserType == "Teacher")
        {
            var teacher = user as SmartClassRoom.Web.Models.Entities.Users.Teacher;
            if (teacher != null)
            {
                model.EmployeeCode = teacher.EmployeeCode;
                model.DepartmentId = teacher.DepartmentId;
                model.Title = teacher.Title;
                model.Specialization = teacher.Specialization;
                model.HireDate = teacher.HireDate;
                model.EmploymentStatus = teacher.EmploymentStatus;
            }
        }
        else if (user.UserType == "Student")
        {
            var student = user as SmartClassRoom.Web.Models.Entities.Users.Student;
            if (student != null)
            {
                model.StudentCode = student.StudentCode;
                model.DepartmentId = student.DepartmentId;
                model.LevelId = student.LevelId;
                model.SectionId = student.SectionId;
                model.GroupId = student.GroupId;
                model.EnrollmentDate = student.EnrollmentDate;
                model.AcademicStatus = student.AcademicStatus;
                model.CurrentGPA = student.CurrentGPA;
            }
        }

        await PopulateDropdowns();
        return View(model);
    }

    // POST: Admin/Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserRequest model)
    {
        if (id != model.UserId)
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _userService.UpdateUserAsync(model);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Users/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userService.GetUserRolesAsync(id);
        ViewBag.Roles = roles;

        return View(user);
    }

    // POST: Admin/Users/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _userService.DeactivateUserAsync(id);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "User deactivated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to deactivate user.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Users/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _userService.ActivateUserAsync(id);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "User activated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to activate user.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Users/ResetPassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        // Reset password to default: Pass@123
        const string defaultPassword = "Pass@123";

        var userManager = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SmartClassRoom.Web.Models.Entities.Identity.ApplicationUser>>();

        // Remove existing password
        var removePasswordResult = await userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Failed to reset password.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Add new password
        var addPasswordResult = await userManager.AddPasswordAsync(user, defaultPassword);
        if (addPasswordResult.Succeeded)
        {
            TempData["SuccessMessage"] = $"Password reset successfully to default: {defaultPassword}";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["ErrorMessage"] = "Failed to reset password: " + string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Admin/Users/ResetUDID/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetUDID(int id)
    {
        var result = await _userService.ResetUDIDAsync(id);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "UDID reset successfully. User can now login from a new device.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to reset UDID.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Admin/Users/UploadFacePhoto/5
    public async Task<IActionResult> UploadFacePhoto(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.UserId = id;
        ViewBag.UserName = user.FullName;
        ViewBag.CurrentPhoto = user.ProfilePhotoPath;

        return View();
    }

    // POST: Admin/Users/UploadFacePhoto/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadFacePhoto(int id, IFormFile facePhoto)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        if (facePhoto == null || facePhoto.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a face photo to upload.";
            ViewBag.UserId = id;
            ViewBag.UserName = user.FullName;
            ViewBag.CurrentPhoto = user.ProfilePhotoPath;
            return View();
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(facePhoto.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            TempData["ErrorMessage"] = "Invalid file type. Only JPG, JPEG, and PNG files are allowed.";
            ViewBag.UserId = id;
            ViewBag.UserName = user.FullName;
            ViewBag.CurrentPhoto = user.ProfilePhotoPath;
            return View();
        }

        // Validate file size (5MB max)
        if (facePhoto.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "File size must be less than 5MB.";
            ViewBag.UserId = id;
            ViewBag.UserName = user.FullName;
            ViewBag.CurrentPhoto = user.ProfilePhotoPath;
            return View();
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "faces");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileName = $"{id}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Read image bytes for face recognition
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await facePhoto.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            // Enroll face using the face recognition service
            var enrollResult = await _faceEnrollmentService.EnrollFaceAsync(id, imageBytes);

            if (!enrollResult.Success)
            {
                TempData["ErrorMessage"] = enrollResult.Message;
                ViewBag.UserId = id;
                ViewBag.UserName = user.FullName;
                ViewBag.CurrentPhoto = user.ProfilePhotoPath;
                return View();
            }

            // Delete old photo if exists
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Save the photo file (embedding already saved by EnrollFaceAsync)
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                using var ms = new MemoryStream(imageBytes);
                await ms.CopyToAsync(stream);
            }

            // Reload user to get the updated FaceEmbeddingBase64 from EnrollFaceAsync
            var userManager = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SmartClassRoom.Web.Models.Entities.Identity.ApplicationUser>>();
            var updatedUser = await userManager.FindByIdAsync(id.ToString());

            if (updatedUser == null)
            {
                TempData["ErrorMessage"] = "User not found after enrollment.";
                return RedirectToAction(nameof(Index));
            }

            // Update profile photo path on the reloaded user (preserves the embedding)
            updatedUser.ProfilePhotoPath = $"/uploads/faces/{fileName}";
            updatedUser.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(updatedUser);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Face photo uploaded and enrolled successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to save face photo path.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error uploading photo: {ex.Message}";
        }

        ViewBag.UserId = id;
        ViewBag.UserName = user.FullName;
        ViewBag.CurrentPhoto = user.ProfilePhotoPath;
        return View();
    }

    private async Task PopulateDropdowns()
    {
        // Roles - Using UserService
        ViewBag.Roles = new SelectList(
            await _userService.GetAllRolesAsync(),
            "Roles");

        // User Types
        ViewBag.UserTypes = new SelectList(new[] { "Student", "Teacher" });

        // Departments - Using LookupService
        ViewBag.Departments = new SelectList(
            await _lookupService.GetAllDepartmentsAsync(),
            "Id", "Name");

        // Levels - Using LookupService
        ViewBag.Levels = new SelectList(
            await _lookupService.GetAllLevelsAsync(),
            "Id", "Name");

        // Sections - Using LookupService
        ViewBag.Sections = new SelectList(
            await _lookupService.GetAllSectionsAsync(),
            "Id", "Name");

        // Groups - Using LookupService
        ViewBag.Groups = new SelectList(
            await _lookupService.GetAllGroupsAsync(),
            "Id", "Name");

        // Universities - Using LookupService
        ViewBag.Universities = new SelectList(
            await _lookupService.GetAllUniversitiesAsync(),
            "Id", "Name");
    }
}
