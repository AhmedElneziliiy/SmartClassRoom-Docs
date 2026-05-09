using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Models.ViewModels.Groups;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GroupsController : Controller
{
    private readonly IGroupService _groupService;
    private readonly ILookupService _lookupService;

    public GroupsController(
        IGroupService groupService,
        ILookupService lookupService)
    {
        _groupService = groupService;
        _lookupService = lookupService;
    }

    // GET: Admin/Groups
    public async Task<IActionResult> Index(
        int page = 1,
        int? sectionId = null,
        int? levelId = null,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedGroups = await _groupService.GetGroupsPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            sectionId: sectionId,
            levelId: levelId,
            departmentId: departmentId,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        // Create pagination view model
        var paginationViewModel = PaginationViewModel.Create(
            pagedGroups,
            routeValues: new Dictionary<string, string?>
            {
                { "sectionId", sectionId?.ToString() },
                { "levelId", levelId?.ToString() },
                { "departmentId", departmentId?.ToString() },
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Groups",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedSectionId = sectionId;
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

        // Get sections for filter dropdown (filtered by level if selected)
        var sections = levelId.HasValue
            ? await _lookupService.GetSectionsByLevelAsync(levelId.Value)
            : await _lookupService.GetAllSectionsAsync();

        ViewBag.Sections = new SelectList(sections, "Id", "Name");

        return View(pagedGroups.Items);
    }

    // GET: Admin/Groups/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();

        var model = new CreateGroupRequest
        {
            IsActive = true
        };

        return View(model);
    }

    // POST: Admin/Groups/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupRequest model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _groupService.CreateGroupAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Groups/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        if (group == null)
        {
            TempData["ErrorMessage"] = "Group not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns();

        var model = new EditGroupRequest
        {
            Id = group.Id,
            Name = group.Name,
            Code = group.Code,
            SectionId = group.SectionId,
            Capacity = group.Capacity,
            IsActive = group.IsActive
        };

        return View(model);
    }

    // POST: Admin/Groups/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditGroupRequest model)
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

        var result = await _groupService.UpdateGroupAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Groups/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var group = await _groupService.GetGroupWithDetailsAsync(id);
        if (group == null)
        {
            TempData["ErrorMessage"] = "Group not found.";
            return RedirectToAction(nameof(Index));
        }

        var statistics = await _groupService.GetGroupStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(group);
    }

    // POST: Admin/Groups/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _groupService.DeactivateGroupAsync(id);

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

    // POST: Admin/Groups/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _groupService.ActivateGroupAsync(id);

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

    // AJAX: Get sections by level for cascading dropdown
    [HttpGet]
    public async Task<IActionResult> GetSectionsByLevel(int levelId)
    {
        var sections = await _lookupService.GetSectionsByLevelAsync(levelId);
        return Json(sections.Select(s => new { id = s.Id, name = s.Name }));
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

        // Get all sections for dropdown (will be filtered by JavaScript)
        ViewBag.Sections = new SelectList(
            await _lookupService.GetAllSectionsAsync(),
            "Id",
            "Name");
    }
}
