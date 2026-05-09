using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Models.ViewModels.Departments;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly ILookupService _lookupService;

    public DepartmentsController(
        IDepartmentService departmentService,
        ILookupService lookupService)
    {
        _departmentService = departmentService;
        _lookupService = lookupService;
    }

    // GET: Admin/Departments
    public async Task<IActionResult> Index(
        int page = 1,
        int? universityId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedDepartments = await _departmentService.GetDepartmentsPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            universityId: universityId,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        // Create pagination view model
        var paginationViewModel = PaginationViewModel.Create(
            pagedDepartments,
            routeValues: new Dictionary<string, string?>
            {
                { "universityId", universityId?.ToString() },
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Departments",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedUniversityId = universityId;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        // Get universities for filter dropdown
        ViewBag.Universities = new SelectList(
            await _lookupService.GetAllUniversitiesAsync(),
            "Id",
            "Name");

        return View(pagedDepartments.Items);
    }

    // GET: Admin/Departments/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();

        var model = new CreateDepartmentRequest
        {
            IsActive = true
        };

        return View(model);
    }

    // POST: Admin/Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentRequest model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _departmentService.CreateDepartmentAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Departments/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        if (department == null)
        {
            TempData["ErrorMessage"] = "Department not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns(id);

        var model = new EditDepartmentRequest
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            UniversityId = department.UniversityId,
            HeadOfDepartmentId = department.HeadOfDepartmentId,
            IsActive = department.IsActive
        };

        return View(model);
    }

    // POST: Admin/Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditDepartmentRequest model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid department ID.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(id);
            return View(model);
        }

        var result = await _departmentService.UpdateDepartmentAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns(id);
        return View(model);
    }

    // GET: Admin/Departments/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var department = await _departmentService.GetDepartmentWithDetailsAsync(id);
        if (department == null)
        {
            TempData["ErrorMessage"] = "Department not found.";
            return RedirectToAction(nameof(Index));
        }

        // Get statistics
        var statistics = await _departmentService.GetDepartmentStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        // Get available teachers for head assignment
        var availableTeachers = await _departmentService.GetAvailableHeadsOfDepartmentAsync(
            id,
            department.HeadOfDepartmentId);

        ViewBag.AvailableTeachers = new SelectList(
            availableTeachers,
            "Id",
            "FullName",
            department.HeadOfDepartmentId);

        return View(department);
    }

    // POST: Admin/Departments/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _departmentService.DeactivateDepartmentAsync(id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Departments/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _departmentService.ActivateDepartmentAsync(id);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Departments/AssignHead
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignHead(int departmentId, int? teacherId)
    {
        var result = await _departmentService.AssignHeadOfDepartmentAsync(departmentId, teacherId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Details), new { id = departmentId });
    }

    private async Task PopulateDropdowns(int? departmentId = null)
    {
        // Universities
        ViewBag.Universities = new SelectList(
            await _lookupService.GetAllUniversitiesAsync(),
            "Id",
            "Name");

        // Teachers (if editing and department exists)
        if (departmentId.HasValue)
        {
            var availableTeachers = await _departmentService.GetAvailableHeadsOfDepartmentAsync(
                departmentId.Value);

            ViewBag.Teachers = new SelectList(
                availableTeachers,
                "Id",
                "FullName");
        }
    }
}
