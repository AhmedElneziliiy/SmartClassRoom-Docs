using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Attendance;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.ViewModels.Room;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service implementation for managing Room entities and their ESP devices
/// </summary>
public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Room>> GetRoomsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? building = null,
        string? roomType = null,
        string? status = null,
        bool? hasDevices = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Get all rooms with related data
        var allRooms = await _unitOfWork.Rooms
            .GetAllAsync("ESPDevices", "Sessions", "ScheduledSlots");

        // Apply filtering
        IEnumerable<Room> filtered = allRooms;

        if (!string.IsNullOrWhiteSpace(building))
        {
            filtered = filtered.Where(r => r.Building == building);
        }

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            filtered = filtered.Where(r => r.RoomType == roomType);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(r => r.Status == status);
        }

        if (hasDevices.HasValue)
        {
            if (hasDevices.Value)
            {
                filtered = filtered.Where(r => r.ESPDevices.Any());
            }
            else
            {
                filtered = filtered.Where(r => !r.ESPDevices.Any());
            }
        }

        if (isActive.HasValue)
        {
            filtered = filtered.Where(r => r.IsActive == isActive.Value);
        }

        // Apply sorting
        IEnumerable<Room> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("number", true) => filtered.OrderBy(r => r.Number),
            ("number", false) => filtered.OrderByDescending(r => r.Number),
            ("name", true) => filtered.OrderBy(r => r.Name),
            ("name", false) => filtered.OrderByDescending(r => r.Name),
            ("building", true) => filtered.OrderBy(r => r.Building),
            ("building", false) => filtered.OrderByDescending(r => r.Building),
            ("capacity", true) => filtered.OrderBy(r => r.Capacity),
            ("capacity", false) => filtered.OrderByDescending(r => r.Capacity),
            ("roomtype", true) => filtered.OrderBy(r => r.RoomType),
            ("roomtype", false) => filtered.OrderByDescending(r => r.RoomType),
            ("status", true) => filtered.OrderBy(r => r.Status),
            ("status", false) => filtered.OrderByDescending(r => r.Status),
            _ => filtered.OrderBy(r => r.Number) // Default sort by room number
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<Room>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        return await _unitOfWork.Rooms.GetByIdAsync(id);
    }

    public async Task<Room?> GetRoomWithDetailsAsync(int id)
    {
        return await _unitOfWork.Rooms.GetByIdAsync(id, "ESPDevices", "Sessions", "ScheduledSlots");
    }

    public async Task<(bool Success, string Message)> CreateRoomAsync(CreateRoomRequest request)
    {
        try
        {
            // Check for duplicate room number
            var existingRooms = await _unitOfWork.Rooms.GetAllAsync();
            if (existingRooms.Any(r => r.Number.Equals(request.Number, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, $"Room with number '{request.Number}' already exists.");
            }

            var room = new Room
            {
                Number = request.Number,
                Name = request.Name,
                RoomType = request.RoomType,
                Capacity = request.Capacity,
                Building = request.Building,
                Floor = request.Floor,
                Equipment = request.Equipment,
                Status = request.Status ?? "Available",
                IsActive = request.IsActive
            };

            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Room '{room.Number}' created successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating room: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateRoomAsync(EditRoomRequest request)
    {
        try
        {
            var room = await GetRoomByIdAsync(request.Id);
            if (room == null)
            {
                return (false, "Room not found.");
            }

            // Check for duplicate room number (excluding current room)
            var existingRooms = await _unitOfWork.Rooms.GetAllAsync();
            if (existingRooms.Any(r => r.Id != request.Id &&
                r.Number.Equals(request.Number, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, $"Room with number '{request.Number}' already exists.");
            }

            // Update properties
            room.Number = request.Number;
            room.Name = request.Name;
            room.RoomType = request.RoomType;
            room.Capacity = request.Capacity;
            room.Building = request.Building;
            room.Floor = request.Floor;
            room.Equipment = request.Equipment;
            room.Status = request.Status;
            room.IsActive = request.IsActive;
            // Note: DeviceId is not updated here - it's managed by device operations

            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Room '{room.Number}' updated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating room: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeactivateRoomAsync(int id)
    {
        try
        {
            var room = await GetRoomWithDetailsAsync(id);
            if (room == null)
            {
                return (false, "Room not found.");
            }

            if (!room.IsActive)
            {
                return (false, "Room is already deactivated.");
            }

            // Check for active sessions
            var activeSessions = room.Sessions.Count(s => s.Status == "Active" || s.Status == "InProgress");
            if (activeSessions > 0)
            {
                return (false, $"Cannot deactivate room. It has {activeSessions} active session(s).");
            }

            room.IsActive = false;
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Room '{room.Number}' deactivated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error deactivating room: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ActivateRoomAsync(int id)
    {
        try
        {
            var room = await GetRoomByIdAsync(id);
            if (room == null)
            {
                return (false, "Room not found.");
            }

            if (room.IsActive)
            {
                return (false, "Room is already active.");
            }

            room.IsActive = true;
            _unitOfWork.Rooms.Update(room);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Room '{room.Number}' activated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error activating room: {ex.Message}");
        }
    }

    public async Task<RoomStatistics> GetRoomStatisticsAsync(int id)
    {
        var room = await GetRoomWithDetailsAsync(id);
        if (room == null)
        {
            return new RoomStatistics { RoomId = id };
        }

        return new RoomStatistics
        {
            RoomId = id,
            SessionsCount = room.Sessions.Count,
            ActiveSessionsCount = room.Sessions.Count(s => s.Status == "Active" || s.Status == "InProgress"),
            DevicesCount = room.ESPDevices.Count,
            ScheduledSlotsCount = room.ScheduledSlots.Count
        };
    }

    public async Task<IEnumerable<string>> GetDistinctBuildingsAsync()
    {
        var rooms = await _unitOfWork.Rooms.GetAllAsync();
        return rooms
            .Where(r => !string.IsNullOrWhiteSpace(r.Building))
            .Select(r => r.Building!)
            .Distinct()
            .OrderBy(b => b);
    }

    public async Task<IEnumerable<string>> GetDistinctRoomTypesAsync()
    {
        var rooms = await _unitOfWork.Rooms.GetAllAsync();
        return rooms
            .Where(r => !string.IsNullOrWhiteSpace(r.RoomType))
            .Select(r => r.RoomType!)
            .Distinct()
            .OrderBy(t => t);
    }

    #region Device Management

    public async Task<(bool Success, string Message)> AddDeviceToRoomAsync(AddDeviceRequest request)
    {
        try
        {
            var room = await GetRoomWithDetailsAsync(request.RoomId);
            if (room == null)
            {
                return (false, "Room not found.");
            }

            // Check if room already has a DeviceId
            if (!string.IsNullOrEmpty(room.DeviceId))
            {
                // Room already has devices - new device must have same DeviceId
                if (!request.DeviceId.Equals(room.DeviceId, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"This room uses Device ID '{room.DeviceId}'. All devices must have the same modulation number.");
                }
            }
            else
            {
                // First device - set room's DeviceId
                room.DeviceId = request.DeviceId;
                _unitOfWork.Rooms.Update(room);
            }

            // Check if device with same DeviceId and MacAddress already exists
            var allDevices = await _unitOfWork.Repository<ESPDevice>().GetAllAsync();
            if (!string.IsNullOrWhiteSpace(request.MacAddress) &&
                allDevices.Any(d => d.MacAddress == request.MacAddress))
            {
                return (false, $"A device with MAC address '{request.MacAddress}' already exists.");
            }

            // Create and add ESP device
            var device = new ESPDevice
            {
                DeviceId = request.DeviceId,
                DeviceName = "SmartClassRoom", // Fixed name per requirements
                RoomId = request.RoomId,
                MacAddress = request.MacAddress,
                IPAddress = request.IPAddress,
                Status = "Active",
                RegisteredAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ESPDevice>().AddAsync(device);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Device added to room '{room.Number}' successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error adding device: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> RemoveDeviceFromRoomAsync(int roomId, int deviceId)
    {
        try
        {
            var room = await GetRoomWithDetailsAsync(roomId);
            if (room == null)
            {
                return (false, "Room not found.");
            }

            var device = room.ESPDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device == null)
            {
                return (false, "Device not found in this room.");
            }

            _unitOfWork.Repository<ESPDevice>().Delete(device);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Device removed from room successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error removing device: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ESPDevice>> GetRoomDevicesAsync(int roomId)
    {
        var room = await GetRoomWithDetailsAsync(roomId);
        return room?.ESPDevices ?? Enumerable.Empty<ESPDevice>();
    }

    #endregion
}
