using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IRoomRepository : IRepository<Room>
{
    Task<IEnumerable<Room>> GetActiveRoomsAsync();
    Task<IEnumerable<Room>> GetByRoomTypeAsync(string roomType);
    Task<IEnumerable<Room>> GetByBuildingAsync(string building);
    Task<IEnumerable<Room>> GetRoomsByMinCapacityAsync(int minCapacity);

    /// <summary>
    /// Get available rooms for a specific day/time slot within a term
    /// </summary>
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(
        string dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        int termId,
        string? roomType = null,
        int? minCapacity = null);

    Task<Room?> GetRoomWithScheduleAsync(int roomId, int termId);
}
