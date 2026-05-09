using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Models.ViewModels.Levels;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class LevelsController : Controller
{
    private readonly ILevelService _levelService;
    private readonly ILookupService _lookupService;

    public LevelsController(
        ILevelService levelService,
        ILookupService lookupService)
    {
        _levelService = levelService;
        _lookupService = lookupService;
    }

    // GET: Admin/Levels
    public async Task<IActionResult> Index(
        int page = 1,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedLevels = await _levelService.GetLevelsPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            departmentId: departmentId,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        // Create pagination view model
        var paginationViewModel = PaginationViewModel.Create(
            pagedLevels,
            routeValues: new Dictionary<string, string?>
            {
                { "departmentId", departmentId?.ToString() },
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Levels",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedDepartmentId = departmentId;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        // Get departments for filter dropdown
        ViewBag.Departments = new SelectList(
            await _lookupService.GetAllDepartmentsAsync(),
            "Id",
            "Name");

        return View(pagedLevels.Items);
    }

    // GET: Admin/Levels/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();

        var model = new CreateLevelRequest
        {
            IsActive = true
        };

        return View(model);
    }

    // POST: Admin/Levels/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLevelRequest model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _levelService.CreateLevelAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Levels/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var level = await _levelService.GetLevelByIdAsync(id);
        if (level == null)
        {
            TempData["ErrorMessage"] = "Level not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns();

        var model = new EditLevelRequest
        {
            Id = level.Id,
            Name = level.Name,
            LevelNumber = level.LevelNumber,
            DepartmentId = level.DepartmentId,
            IsActive = level.IsActive
        };

        return View(model);
    }

    // POST: Admin/Levels/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditLevelRequest model)
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

        var result = await _levelService.UpdateLevelAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Levels/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var level = await _levelService.GetLevelWithDetailsAsync(id);
        if (level == null)
        {
            TempData["ErrorMessage"] = "Level not found.";
            return RedirectToAction(nameof(Index));
        }

        var statistics = await _levelService.GetLevelStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(level);
    }

    // POST: Admin/Levels/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _levelService.DeactivateLevelAsync(id);

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

    // POST: Admin/Levels/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _levelService.ActivateLevelAsync(id);

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

    private async Task PopulateDropdowns()
    {
        // Get departments for dropdown
        ViewBag.Departments = new SelectList(
            await _lookupService.GetAllDepartmentsAsync(),
            "Id",
            "Name");
    }
}
