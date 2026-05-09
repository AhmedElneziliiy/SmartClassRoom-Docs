using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Models.ViewModels.Terms;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TermsController : Controller
{
    private readonly ITermService _termService;

    public TermsController(ITermService termService)
    {
        _termService = termService;
    }

    // GET: Admin/Terms
    public async Task<IActionResult> Index(
        int page = 1,
        bool? isActive = null,
        string? status = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedTerms = await _termService.GetTermsPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            isActive: isActive,
            status: status,
            sortBy: sortBy,
            ascending: ascending);

        // Create pagination view model
        var paginationViewModel = PaginationViewModel.Create(
            pagedTerms,
            routeValues: new Dictionary<string, string?>
            {
                { "isActive", isActive?.ToString() },
                { "status", status },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Terms",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.SelectedStatus = status;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        return View(pagedTerms.Items);
    }

    // GET: Admin/Terms/Create
    public IActionResult Create()
    {
        var model = new CreateTermRequest
        {
            IsActive = true,
            Status = "Upcoming"
        };

        return View(model);
    }

    // POST: Admin/Terms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTermRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _termService.CreateTermAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return View(model);
    }

    // GET: Admin/Terms/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var term = await _termService.GetTermByIdAsync(id);
        if (term == null)
        {
            TempData["ErrorMessage"] = "Term not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new EditTermRequest
        {
            Id = term.Id,
            Name = term.Name,
            Code = term.Code,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            Status = term.Status,
            IsActive = term.IsActive
        };

        return View(model);
    }

    // POST: Admin/Terms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditTermRequest model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid request.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _termService.UpdateTermAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return View(model);
    }

    // GET: Admin/Terms/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var term = await _termService.GetTermWithDetailsAsync(id);
        if (term == null)
        {
            TempData["ErrorMessage"] = "Term not found.";
            return RedirectToAction(nameof(Index));
        }

        var statistics = await _termService.GetTermStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(term);
    }

    // POST: Admin/Terms/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _termService.DeactivateTermAsync(id);

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

    // POST: Admin/Terms/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _termService.ActivateTermAsync(id);

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
