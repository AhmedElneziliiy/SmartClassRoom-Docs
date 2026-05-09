using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Attendance;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.ViewModels.Room;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for managing Room entities and their ESP devices
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Get paginated list of rooms with optional filtering and sorting
    /// </summary>
    Task<PagedResult<Room>> GetRoomsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? building = null,
        string? roomType = null,
        string? status = null,
        bool? hasDevices = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Get room by ID
    /// </summary>
    Task<Room?> GetRoomByIdAsync(int id);

    /// <summary>
    /// Get room with all related details loaded (devices, sessions, scheduled slots)
    /// </summary>
    Task<Room?> GetRoomWithDetailsAsync(int id);

    /// <summary>
    /// Create a new room
    /// </summary>
    Task<(bool Success, string Message)> CreateRoomAsync(CreateRoomRequest request);

    /// <summary>
    /// Update an existing room
    /// </summary>
    Task<(bool Success, string Message)> UpdateRoomAsync(EditRoomRequest request);

    /// <summary>
    /// Deactivate a room (soft delete)
    /// </summary>
    Task<(bool Success, string Message)> DeactivateRoomAsync(int id);

    /// <summary>
    /// Activate a room
    /// </summary>
    Task<(bool Success, string Message)> ActivateRoomAsync(int id);

    /// <summary>
    /// Get statistics for a specific room
    /// </summary>
    Task<RoomStatistics> GetRoomStatisticsAsync(int id);

    /// <summary>
    /// Get all distinct buildings for dropdown
    /// </summary>
    Task<IEnumerable<string>> GetDistinctBuildingsAsync();

    /// <summary>
    /// Get all distinct room types for dropdown
    /// </summary>
    Task<IEnumerable<string>> GetDistinctRoomTypesAsync();

    #region Device Management

    /// <summary>
    /// Add an ESP device to a room.
    /// First device sets the room's DeviceId. Subsequent devices must have matching DeviceId.
    /// </summary>
    Task<(bool Success, string Message)> AddDeviceToRoomAsync(AddDeviceRequest request);

    /// <summary>
    /// Remove an ESP device from a room
    /// </summary>
    Task<(bool Success, string Message)> RemoveDeviceFromRoomAsync(int roomId, int deviceId);

    /// <summary>
    /// Get all ESP devices for a room
    /// </summary>
    Task<IEnumerable<ESPDevice>> GetRoomDevicesAsync(int roomId);

    #endregion
}
