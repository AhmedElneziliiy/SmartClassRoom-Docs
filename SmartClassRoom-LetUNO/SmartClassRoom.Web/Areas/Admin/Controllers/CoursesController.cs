using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Models.ViewModels.Courses;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICourseRepository _courseRepository;

    public CoursesController(
        ICourseService courseService,
        IUnitOfWork unitOfWork,
        ICourseRepository courseRepository)
    {
        _courseService = courseService;
        _unitOfWork = unitOfWork;
        _courseRepository = courseRepository;
    }

    // GET: Admin/Courses
    public async Task<IActionResult> Index(
        int page = 1,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedCourses = await _courseService.GetCoursesPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            departmentId: departmentId,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        var paginationViewModel = PaginationViewModel.Create(
            pagedCourses,
            routeValues: new Dictionary<string, string?>
            {
                { "departmentId", departmentId?.ToString() },
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Courses",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;
        ViewBag.DepartmentId = departmentId;
        ViewBag.IsActive = isActive;

        // Load departments for filter dropdown
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
        ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive), "Id", "Name");

        return View(pagedCourses.Items);
    }

    // GET: Admin/Courses/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var course = await _courseService.GetCourseWithDetailsAsync(id);
        if (course == null)
        {
            TempData["ErrorMessage"] = "Course not found.";
            return RedirectToAction(nameof(Index));
        }

        var statistics = await _courseService.GetCourseStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(course);
    }

    // GET: Admin/Courses/Create
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();
        return View();
    }

    // POST: Admin/Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(request);
        }

        var (success, message) = await _courseService.CreateCourseAsync(request);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = message;
        await LoadDropdownsAsync();
        return View(request);
    }

    // GET: Admin/Courses/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null)
        {
            TempData["ErrorMessage"] = "Course not found.";
            return RedirectToAction(nameof(Index));
        }

        var request = new EditCourseRequest
        {
            Id = course.Id,
            Name = course.Name,
            Code = course.Code,
            Description = course.Description,
            Credits = course.Credits,
            CourseType = course.CourseType,
            DepartmentId = course.DepartmentId,
            PrerequisiteCourseId = course.PrerequisiteCourseId,
            IsActive = course.IsActive
        };

        await LoadDropdownsAsync(course.DepartmentId);
        return View(request);
    }

    // POST: Admin/Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCourseRequest request)
    {
        if (id != request.Id)
        {
            TempData["ErrorMessage"] = "Invalid course ID.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(request.DepartmentId);
            return View(request);
        }

        var (success, message) = await _courseService.UpdateCourseAsync(request);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = message;
        await LoadDropdownsAsync(request.DepartmentId);
        return View(request);
    }

    // POST: Admin/Courses/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var (success, message) = await _courseService.DeactivateCourseAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Courses/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var (success, message) = await _courseService.ActivateCourseAsync(id);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Helper method to load dropdowns
    private async Task LoadDropdownsAsync(int? selectedDepartmentId = null)
    {
        // Load departments
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
        ViewBag.Departments = new SelectList(
            departments.Where(d => d.IsActive).OrderBy(d => d.Name),
            "Id",
            "Name",
            selectedDepartmentId);

        // Load courses for prerequisite dropdown
        var courses = await _courseRepository.GetActiveCoursesAsync();
        ViewBag.PrerequisiteCourses = new SelectList(
            courses.OrderBy(c => c.Code),
            "Id",
            "Name",
            null);

        // Course types
        ViewBag.CourseTypes = new SelectList(new[]
        {
            new { Value = "Lecture", Text = "Lecture" },
            new { Value = "Lab", Text = "Lab" },
            new { Value = "Hybrid", Text = "Hybrid" },
            new { Value = "Seminar", Text = "Seminar" },
            new { Value = "Project", Text = "Project" }
        }, "Value", "Text");
    }
}
