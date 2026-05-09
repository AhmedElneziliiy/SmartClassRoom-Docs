using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Models.ViewModels.Room;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RoomsController : Controller
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // GET: Admin/Rooms
    public async Task<IActionResult> Index(
        int page = 1,
        string? building = null,
        string? roomType = null,
        string? status = null,
        bool? hasDevices = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        const int pageSize = 20;

        var pagedRooms = await _roomService.GetRoomsPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            building: building,
            roomType: roomType,
            status: status,
            hasDevices: hasDevices,
            isActive: isActive,
            sortBy: sortBy,
            ascending: ascending);

        // Create pagination view model
        var paginationViewModel = PaginationViewModel.Create(
            pagedRooms,
            routeValues: new Dictionary<string, string?>
            {
                { "building", building },
                { "roomType", roomType },
                { "status", status },
                { "hasDevices", hasDevices?.ToString() },
                { "isActive", isActive?.ToString() },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Rooms",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedBuilding = building;
        ViewBag.SelectedRoomType = roomType;
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedHasDevices = hasDevices;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        await PopulateFilterDropdowns();

        return View(pagedRooms.Items);
    }

    // GET: Admin/Rooms/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();

        var model = new CreateRoomRequest
        {
            IsActive = true,
            Status = "Available"
        };

        return View(model);
    }

    // POST: Admin/Rooms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoomRequest model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var result = await _roomService.CreateRoomAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Rooms/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            TempData["ErrorMessage"] = "Room not found.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns();

        var model = new EditRoomRequest
        {
            Id = room.Id,
            Number = room.Number,
            Name = room.Name,
            RoomType = room.RoomType,
            Capacity = room.Capacity,
            Building = room.Building,
            Floor = room.Floor,
            Equipment = room.Equipment,
            Status = room.Status,
            IsActive = room.IsActive,
            DeviceId = room.DeviceId
        };

        return View(model);
    }

    // POST: Admin/Rooms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditRoomRequest model)
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

        var result = await _roomService.UpdateRoomAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        await PopulateDropdowns();
        return View(model);
    }

    // GET: Admin/Rooms/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var room = await _roomService.GetRoomWithDetailsAsync(id);
        if (room == null)
        {
            TempData["ErrorMessage"] = "Room not found.";
            return RedirectToAction(nameof(Index));
        }

        var statistics = await _roomService.GetRoomStatisticsAsync(id);
        ViewBag.Statistics = statistics;

        return View(room);
    }

    // POST: Admin/Rooms/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _roomService.DeactivateRoomAsync(id);

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

    // POST: Admin/Rooms/Activate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _roomService.ActivateRoomAsync(id);

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

    #region Device Management

    // POST: Admin/Rooms/AddDevice
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDevice(AddDeviceRequest model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid device data. Please fill all required fields.";
            return RedirectToAction(nameof(Details), new { id = model.RoomId });
        }

        var result = await _roomService.AddDeviceToRoomAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.RoomId });
    }

    // POST: Admin/Rooms/RemoveDevice
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDevice(int roomId, int deviceId)
    {
        var result = await _roomService.RemoveDeviceFromRoomAsync(roomId, deviceId);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Details), new { id = roomId });
    }

    #endregion

    #region Helper Methods

    private async Task PopulateDropdowns()
    {
        // Populate room types dropdown
        ViewBag.RoomTypes = new SelectList(new[]
        {
            "Lecture",
            "Lab",
            "Auditorium",
            "Seminar",
            "Workshop",
            "Computer Lab",
            "Conference"
        });

        // Populate status dropdown
        ViewBag.Statuses = new SelectList(new[]
        {
            "Available",
            "Maintenance",
            "Reserved",
            "Unavailable"
        });

        // Get distinct buildings for dropdown
        var buildings = await _roomService.GetDistinctBuildingsAsync();
        ViewBag.Buildings = new SelectList(buildings);
    }

    private async Task PopulateFilterDropdowns()
    {
        // Get distinct buildings for filter
        var buildings = await _roomService.GetDistinctBuildingsAsync();
        ViewBag.Buildings = new SelectList(buildings);

        // Get distinct room types for filter
        var roomTypes = await _roomService.GetDistinctRoomTypesAsync();
        ViewBag.RoomTypes = new SelectList(roomTypes);

        // Populate status dropdown
        ViewBag.Statuses = new SelectList(new[]
        {
            "Available",
            "Maintenance",
            "Reserved",
            "Unavailable"
        });
    }

    #endregion
}
