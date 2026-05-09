using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Models.ViewModels.Universities;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UniversitiesController : Controller
{
    private readonly IUniversityService _universityService;

    public UniversitiesController(IUniversityService universityService)
    {
        _universityService = universityService;
    }

    // GET: Admin/Universities
    public async Task<IActionResult> Index(
        int page = 1,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedUniversities = await _universityService.GetUniversitiesPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        // Create pagination view model
        var paginationViewModel = PaginationViewModel.Create(
            pagedUniversities,
            routeValues: new Dictionary<string, string?>
            {
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Universities",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        return View(pagedUniversities.Items);
    }

    // GET: Admin/Universities/Create
    public IActionResult Create()
    {
        var model = new CreateUniversityRequest
        {
            IsActive = true
        };

        return View(model);
    }

    // POST: Admin/Universities/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUniversityRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _universityService.CreateUniversityAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return View(model);
    }

    // GET: Admin/Universities/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var university = await _universityService.GetUniversityByIdAsync(id);
        if (university == null)
        {
            TempData["ErrorMessage"] = "University not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new EditUniversityRequest
        {
            Id = university.Id,
            Name = university.Name,
            Code = university.Code,
            Address = university.Address,
            Phone = university.Phone,
            Email = university.Email,
            Website = university.Website,
            IsActive = university.IsActive
        };

        return View(model);
    }

    // POST: Admin/Universities/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUniversityRequest model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid university ID.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _universityService.UpdateUniversityAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return View(model);
    }

    // GET: Admin/Universities/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var university = await _universityService.GetUniversityWithDepartmentsAsync(id);
        if (university == null)
        {
            TempData["ErrorMessage"] = "University not found.";
            return RedirectToAction(nameof(Index));
        }

        // Get statistics
        var statistics = await _universityService.GetUniversityStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(university);
    }

    // POST: Admin/Universities/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _universityService.DeactivateUniversityAsync(id);

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

    // POST: Admin/Universities/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _universityService.ActivateUniversityAsync(id);

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
}
