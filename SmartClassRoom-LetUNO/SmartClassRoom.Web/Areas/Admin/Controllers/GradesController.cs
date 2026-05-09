using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Models.ViewModels.Grading;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class GradesController : Controller
{
    private readonly IGradingService _gradingService;
    private readonly IUnitOfWork _unitOfWork;

    public GradesController(IGradingService gradingService, IUnitOfWork unitOfWork)
    {
        _gradingService = gradingService;
        _unitOfWork = unitOfWork;
    }

    // GET: /Admin/Grades
    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        string? search = null,
        int? termId = null,
        string? status = null,
        bool? hasComponents = null,
        string? sortBy = null,
        bool ascending = false)
    {
        const int pageSize = 20;

        // Get current user
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        IEnumerable<CourseOffering> offerings;

        // Filter by teacher if not admin
        if (!isAdmin)
        {
            offerings = await _unitOfWork.CourseOfferings.GetByTeacherAsync(userId);
        }
        else
        {
            offerings = await _unitOfWork.CourseOfferings.GetAllAsync();
        }

        // Load related data for each offering
        var offeringsWithDetails = new List<CourseOffering>();
        foreach (var offering in offerings)
        {
            var offeringWithDetails = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offering.Id);
            if (offeringWithDetails != null)
            {
                offeringsWithDetails.Add(offeringWithDetails);
            }
        }

        // Apply filters
        var filteredOfferings = offeringsWithDetails.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            filteredOfferings = filteredOfferings.Where(o =>
                o.Course.Code.ToLower().Contains(search) ||
                o.Course.Name.ToLower().Contains(search) ||
                o.Teacher.FullName.ToLower().Contains(search));
        }

        if (termId.HasValue)
        {
            filteredOfferings = filteredOfferings.Where(o => o.TermId == termId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filteredOfferings = filteredOfferings.Where(o => o.Status == status);
        }

        if (hasComponents.HasValue)
        {
            if (hasComponents.Value)
            {
                filteredOfferings = filteredOfferings.Where(o => o.GradeComponents != null && o.GradeComponents.Any());
            }
            else
            {
                filteredOfferings = filteredOfferings.Where(o => o.GradeComponents == null || !o.GradeComponents.Any());
            }
        }

        // Apply sorting
        filteredOfferings = sortBy?.ToLower() switch
        {
            "code" => ascending ? filteredOfferings.OrderBy(o => o.Course.Code) : filteredOfferings.OrderByDescending(o => o.Course.Code),
            "name" => ascending ? filteredOfferings.OrderBy(o => o.Course.Name) : filteredOfferings.OrderByDescending(o => o.Course.Name),
            "term" => ascending ? filteredOfferings.OrderBy(o => o.Term.StartDate) : filteredOfferings.OrderByDescending(o => o.Term.StartDate),
            "teacher" => ascending ? filteredOfferings.OrderBy(o => o.Teacher.FullName) : filteredOfferings.OrderByDescending(o => o.Teacher.FullName),
            "students" => ascending ? filteredOfferings.OrderBy(o => o.Enrollments.Count(e => e.Status == "Enrolled")) : filteredOfferings.OrderByDescending(o => o.Enrollments.Count(e => e.Status == "Enrolled")),
            "components" => ascending ? filteredOfferings.OrderBy(o => o.GradeComponents?.Count ?? 0) : filteredOfferings.OrderByDescending(o => o.GradeComponents?.Count ?? 0),
            _ => filteredOfferings.OrderByDescending(o => o.Term.StartDate)
        };

        var offeringsList = filteredOfferings.ToList();

        // Apply pagination
        var totalCount = offeringsList.Count;
        var paginatedOfferings = offeringsList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Create pagination view model
        var pagedResult = new PagedResult<CourseOffering>
        {
            Items = paginatedOfferings,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        var paginationViewModel = PaginationViewModel.Create(
            pagedResult,
            routeValues: new Dictionary<string, string?>
            {
                { "search", search },
                { "termId", termId?.ToString() },
                { "status", status },
                { "hasComponents", hasComponents?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Grades",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;

        // Pass filter values to view
        ViewBag.Search = search;
        ViewBag.SelectedTermId = termId;
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedHasComponents = hasComponents;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        // Get all terms for filter dropdown
        var terms = await _unitOfWork.Terms.GetAllAsync();
        ViewBag.Terms = terms.OrderByDescending(t => t.StartDate).ToList();

        return View(paginatedOfferings);
    }

    // GET: /Admin/Grades/ConfigureComponents/{offeringId}
    [HttpGet]
    public async Task<IActionResult> ConfigureComponents(int id)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(id);
        if (offering == null)
            return NotFound();

        var components = await _gradingService.GetGradeComponentsAsync(id);
        var templates = await _gradingService.GetTemplatesAsync(offering.Course.DepartmentId);

        // Check if there are existing grades
        var existingGrades = await _unitOfWork.Grades.GetGradesByOfferingAsync(id);
        var hasExistingGrades = existingGrades.Any();
        var existingGradesCount = existingGrades.Count();

        ViewBag.Offering = offering;
        ViewBag.Templates = templates;
        ViewBag.Components = components;
        ViewBag.HasExistingGrades = hasExistingGrades;
        ViewBag.ExistingGradesCount = existingGradesCount;

        return View();
    }

    // POST: /Admin/Grades/SaveComponents
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveComponents(int offeringId, List<GradeComponentDto> components)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid component data";
            return RedirectToAction(nameof(ConfigureComponents), new { id = offeringId });
        }

        var (success, message) = await _gradingService.SaveGradeComponentsAsync(offeringId, components);

        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(GradeSheet), new { id = offeringId });
    }

    // GET: /Admin/Grades/GradeSheet/{offeringId}
    [HttpGet]
    public async Task<IActionResult> GradeSheet(int id)
    {
        try
        {
            var gradeSheet = await _gradingService.GetGradeSheetAsync(id);
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(id);

            ViewBag.Offering = offering;

            return View(gradeSheet);
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "CourseOfferings", new { id });
        }
    }

    // POST: /Admin/Grades/SaveGrade (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGrade([FromBody] SaveGradeDto gradeDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid grade data" });

        var userId = GetCurrentUserId();
        gradeDto.EnteredBy = userId;

        var (success, message) = await _gradingService.SaveGradeAsync(gradeDto);

        if (success)
            return Ok(new { message });
        else
            return BadRequest(new { message });
    }

    // POST: /Admin/Grades/CalculateFinalGrades/{offeringId}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalculateFinalGrades(int id)
    {
        var (success, message, result) = await _gradingService.CalculateFinalGradesAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            if (result != null && result.Warnings.Any())
            {
                TempData["WarningMessage"] = string.Join("<br/>", result.Warnings);
            }
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(GradeSheet), new { id });
    }

    // GET: /Admin/Grades/Templates
    [HttpGet]
    public async Task<IActionResult> Templates()
    {
        var templates = await _gradingService.GetTemplatesAsync();
        return View(templates);
    }

    // POST: /Admin/Grades/SaveTemplate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTemplate(SaveTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid template data";
            return RedirectToAction(nameof(Templates));
        }

        request.CreatedBy = GetCurrentUserId();

        var (success, message, templateId) = await _gradingService.SaveTemplateAsync(request);

        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Templates));
    }

    // POST: /Admin/Grades/ApplyTemplate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyTemplate(int offeringId, int templateId)
    {
        var (success, message) = await _gradingService.ApplyTemplateAsync(offeringId, templateId);

        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(GradeSheet), new { id = offeringId });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}
