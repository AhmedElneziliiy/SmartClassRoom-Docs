using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Models.ViewModels.Sections;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SectionsController : Controller
{
    private readonly ISectionService _sectionService;
    private readonly ILookupService _lookupService;

    public SectionsController(
        ISectionService sectionService,
        ILookupService lookupService)
    {
        _sectionService = sectionService;
        _lookupService = lookupService;
    }

    // GET: Admin/Sections
    public async Task<IActionResult> Index(
        int page = 1,
        int? levelId = null,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedSections = await _sectionService.GetSectionsPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            levelId: levelId,
            departmentId: departmentId,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        // Create pagination view model
        var paginationViewModel = PaginationViewModel.Create(
            pagedSections,
            routeValues: new Dictionary<string, string?>
            {
                { "levelId", levelId?.ToString() },
                { "departmentId", departmentId?.ToString() },
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Sections",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedLevelId = levelId;
        ViewBag.SelectedDepartmentId = departmentId;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        // Get departments for filter dropdown
        ViewBag.Departments = new SelectList(
            await _lookupService.GetAllDepartmentsAsync(),
            "Id",
            "Name");

        // Get levels for filter dropdown (filtered by department if selected)
        var levels = departmentId.HasValue
            ? await _lookupService.GetLevelsByDepartmentAsync(departmentId.Value)
            : await _lookupService.GetAllLevelsAsync();

        ViewBag.Levels = new SelectList(levels, "Id", "Name");

        return View(pagedSections.Items);
    }

    // GET: Admin/Sections/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();

        var model = new CreateSectionRequest
        {
            IsActive = true
        };

        return View(model);
    }

    // POST: Admin/Sections/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSectionRequest model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _sectionService.CreateSectionAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Sections/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var section = await _sectionService.GetSectionByIdAsync(id);
        if (section == null)
        {
            TempData["ErrorMessage"] = "Section not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns();

        var model = new EditSectionRequest
        {
            Id = section.Id,
            Name = section.Name,
            Code = section.Code,
            LevelId = section.LevelId,
            Capacity = section.Capacity,
            IsActive = section.IsActive
        };

        return View(model);
    }

    // POST: Admin/Sections/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditSectionRequest model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _sectionService.UpdateSectionAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Sections/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var section = await _sectionService.GetSectionWithDetailsAsync(id);
        if (section == null)
        {
            TempData["ErrorMessage"] = "Section not found.";
            return RedirectToAction(nameof(Index));
        }

        var statistics = await _sectionService.GetSectionStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(section);
    }

    // POST: Admin/Sections/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _sectionService.DeactivateSectionAsync(id);

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

    // POST: Admin/Sections/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _sectionService.ActivateSectionAsync(id);

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

    // AJAX: Get levels by department for cascading dropdown
    [HttpGet]
    public async Task<IActionResult> GetLevelsByDepartment(int departmentId)
    {
        var levels = await _lookupService.GetLevelsByDepartmentAsync(departmentId);
        return Json(levels.Select(l => new { id = l.Id, name = l.Name }));
    }

    private async Task PopulateDropdowns()
    {
        // Get departments for dropdown
        ViewBag.Departments = new SelectList(
            await _lookupService.GetAllDepartmentsAsync(),
            "Id",
            "Name");

        // Get all levels for dropdown (will be filtered by JavaScript)
        ViewBag.Levels = new SelectList(
            await _lookupService.GetAllLevelsAsync(),
            "Id",
            "Name");
    }
}
