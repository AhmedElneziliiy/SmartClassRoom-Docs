using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Models.ViewModels.Offerings;
using SmartClassRoom.Web.Services.Interfaces;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Identity;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CourseOfferingsController : Controller
{
    private readonly ICourseOfferingService _offeringService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStudentEnrollmentService _enrollmentService;

    public CourseOfferingsController(
        ICourseOfferingService offeringService,
        IUnitOfWork unitOfWork,
        IStudentEnrollmentService enrollmentService)
    {
        _offeringService = offeringService;
        _unitOfWork = unitOfWork;
        _enrollmentService = enrollmentService;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        int? termId = null,
        int? courseId = null,
        int? teacherId = null,
        int? sectionId = null,
        string? status = null,
        string? sortBy = null,
        bool ascending = true)
    {
        var pagedOfferings = await _offeringService.GetOfferingsPagedAsync(
            pageNumber: page,
            pageSize: 20,
            termId: termId,
            courseId: courseId,
            teacherId: teacherId,
            sectionId: sectionId,
            status: status,
            sortBy: sortBy,
            ascending: ascending);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)pagedOfferings.TotalCount / pagedOfferings.PageSize);
        ViewBag.TermId = termId;
        ViewBag.CourseId = courseId;
        ViewBag.TeacherId = teacherId;
        ViewBag.SectionId = sectionId;
        ViewBag.Status = status;
        ViewBag.SortBy = sortBy;
        ViewBag.Ascending = ascending;

        // Load filter dropdowns
        var terms = await _unitOfWork.Terms.GetAllAsync();
        var courses = await _unitOfWork.Courses.GetAllAsync();
        var teachers = await _unitOfWork.Repository<ApplicationUser>().GetAllAsync();
        var sections = await _unitOfWork.Sections.GetAllAsync();

        ViewBag.Terms = new SelectList(terms.Where(t => t.IsActive).OrderByDescending(t => t.StartDate), "Id", "Name");
        ViewBag.Courses = new SelectList(courses.Where(c => c.IsActive).OrderBy(c => c.Code), "Id", "Name");
        ViewBag.Teachers = new SelectList(teachers.OrderBy(t => t.FullName), "Id", "FullName");
        ViewBag.Sections = new SelectList(sections.Where(s => s.IsActive).OrderBy(s => s.Name), "Id", "Name");

        return View(pagedOfferings.Items);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View(new CreateOfferingRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOfferingRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }

        var (success, message, offeringId) = await _offeringService.CreateOfferingAsync(request);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            await LoadDropdowns();
            return View(request);
        }

        // Handle auto-enrollment if requested
        if (request.AutoEnrollStudents && offeringId.HasValue)
        {
            var (successCount, failureCount, errors) = await _enrollmentService.AutoEnrollStudentsAsync(offeringId.Value);

            if (successCount > 0 && failureCount == 0)
            {
                TempData["SuccessMessage"] = $"{message} {successCount} student(s) enrolled automatically.";
            }
            else if (successCount > 0 && failureCount > 0)
            {
                TempData["SuccessMessage"] = message;
                TempData["WarningMessage"] = $"Auto-enrollment: {successCount} student(s) enrolled, {failureCount} failed. Errors: {string.Join("; ", errors.Take(3))}";
            }
            else if (successCount == 0 && failureCount > 0)
            {
                TempData["SuccessMessage"] = message;
                TempData["ErrorMessage"] = $"Auto-enrollment failed: {string.Join("; ", errors.Take(3))}";
            }
            else
            {
                TempData["SuccessMessage"] = message;
                TempData["InfoMessage"] = "Auto-enrollment: No eligible students found to enroll.";
            }
        }
        else
        {
            TempData["SuccessMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var offering = await _offeringService.GetOfferingWithDetailsAsync(id);
        if (offering == null)
        {
            return NotFound();
        }

        var request = new EditOfferingRequest
        {
            Id = offering.Id,
            CourseId = offering.CourseId,
            TermId = offering.TermId,
            TeacherId = offering.TeacherId,
            SectionId = offering.SectionId,
            SessionsPerWeek = offering.SessionsPerWeek,
            MaxStudents = offering.MaxStudents,
            Notes = offering.Notes,
            Status = offering.Status
        };

        await LoadDropdowns();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditOfferingRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }

        var (success, message) = await _offeringService.UpdateOfferingAsync(request);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            await LoadDropdowns();
            return View(request);
        }

        TempData["SuccessMessage"] = message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var offering = await _offeringService.GetOfferingWithDetailsAsync(id);
        if (offering == null)
        {
            return NotFound();
        }

        var statistics = await _offeringService.GetOfferingStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(offering);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, message) = await _offeringService.CancelOfferingAsync(id);

        if (!success)
        {
            TempData["ErrorMessage"] = message;
        }
        else
        {
            TempData["SuccessMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ==================== ENROLLMENT MANAGEMENT ACTIONS ====================

    /// <summary>
    /// Auto-enroll students based on offering criteria (section or department)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AutoEnroll(int id)
    {
        var (successCount, failureCount, errors) = await _enrollmentService.AutoEnrollStudentsAsync(id);

        if (successCount > 0 && failureCount == 0)
        {
            TempData["SuccessMessage"] = $"Successfully enrolled {successCount} student(s) automatically.";
        }
        else if (successCount > 0 && failureCount > 0)
        {
            TempData["WarningMessage"] = $"Enrolled {successCount} student(s), but {failureCount} failed. Errors: {string.Join("; ", errors.Take(3))}";
        }
        else if (successCount == 0 && failureCount > 0)
        {
            TempData["ErrorMessage"] = $"Auto-enrollment failed: {string.Join("; ", errors.Take(3))}";
        }
        else
        {
            TempData["InfoMessage"] = "No eligible students found to enroll.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Manually enroll selected students
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManualEnroll(int id, List<int> studentIds)
    {
        if (studentIds == null || studentIds.Count == 0)
        {
            TempData["ErrorMessage"] = "No students selected for enrollment.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var (successCount, failureCount, errors) = await _enrollmentService.EnrollStudentsByIdsAsync(id, studentIds);

        if (successCount > 0 && failureCount == 0)
        {
            TempData["SuccessMessage"] = $"Successfully enrolled {successCount} student(s).";
        }
        else if (successCount > 0 && failureCount > 0)
        {
            TempData["WarningMessage"] = $"Enrolled {successCount} student(s), but {failureCount} failed. Errors: {string.Join("; ", errors.Take(3))}";
        }
        else
        {
            TempData["ErrorMessage"] = $"Enrollment failed: {string.Join("; ", errors.Take(3))}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Unenroll a student from a course offering
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int enrollmentId, int offeringId)
    {
        var (success, message) = await _enrollmentService.UnenrollStudentAsync(enrollmentId);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Details), new { id = offeringId });
    }

    /// <summary>
    /// AJAX endpoint - Get eligible students for manual enrollment
    /// </summary>
    [HttpGet]
    public async Task<JsonResult> GetEligibleStudents(int id, string? searchTerm = null)
    {
        var students = await _enrollmentService.GetEligibleStudentsAsync(id, searchTerm);

        var result = students.Select(s => new
        {
            studentId = s.StudentId,
            studentName = s.StudentName,
            studentCode = s.StudentCode,
            email = s.Email,
            departmentName = s.DepartmentName,
            levelName = s.LevelName,
            sectionName = s.SectionName,
            isAlreadyEnrolled = s.IsAlreadyEnrolled,
            isEligible = s.IsEligible
        }).ToList();

        return Json(result);
    }

    // AJAX endpoint to get courses by department
    [HttpGet]
    public async Task<JsonResult> GetCoursesByDepartment(int departmentId)
    {
        var courses = await _unitOfWork.Courses.GetByDepartmentAsync(departmentId);
        var result = courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .Select(c => new { id = c.Id, name = $"{c.Code} - {c.Name}" })
            .ToList();

        return Json(result);
    }

    // AJAX endpoint to get teachers by department
    [HttpGet]
    public async Task<JsonResult> GetTeachersByDepartment(int departmentId)
    {
        var teachers = await _unitOfWork.Repository<ApplicationUser>().GetAllAsync();
        // Note: Teachers don't have direct DepartmentId, so return all for now
        // You can enhance this by filtering based on teacher's assigned courses or departments
        var result = teachers
            .OrderBy(t => t.FullName)
            .Select(t => new { id = t.Id, name = t.FullName })
            .ToList();

        return Json(result);
    }

    private async Task LoadDropdowns()
    {
        var terms = await _unitOfWork.Terms.GetAllAsync();
        var courses = await _unitOfWork.Courses.GetAllAsync();
        var teachers = await _unitOfWork.Repository<ApplicationUser>().GetAllAsync();
        var sections = await _unitOfWork.Sections.GetAllAsync();

        ViewBag.Terms = new SelectList(terms.Where(t => t.IsActive).OrderByDescending(t => t.StartDate), "Id", "Name");
        ViewBag.Courses = new SelectList(courses.Where(c => c.IsActive).OrderBy(c => c.Code), "Id", "Name");
        ViewBag.Teachers = new SelectList(teachers.OrderBy(t => t.FullName), "Id", "FullName");
        ViewBag.Sections = new SelectList(sections.Where(s => s.IsActive).OrderBy(s => s.Name), "Id", "Name");
    }
}
