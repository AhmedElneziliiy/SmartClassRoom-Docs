using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
    {
        return await _dbSet
            .Where(r => r.IsActive && r.Status == "Available")
            .OrderBy(r => r.Number)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetByRoomTypeAsync(string roomType)
    {
        return await _dbSet
            .Where(r => r.IsActive && r.RoomType == roomType)
            .OrderBy(r => r.Capacity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetByBuildingAsync(string building)
    {
        return await _dbSet
            .Where(r => r.IsActive && r.Building == building)
            .OrderBy(r => r.Number)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetRoomsByMinCapacityAsync(int minCapacity)
    {
        return await _dbSet
            .Where(r => r.IsActive && r.Capacity >= minCapacity)
            .OrderBy(r => r.Capacity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(
        string dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        int termId,
        string? roomType = null,
        int? minCapacity = null)
    {
        // 1. Start with active rooms
        var activeRoomsQuery = _dbSet
            .Where(r => r.IsActive && r.Status == "Available");

        // 2. Apply filters
        if (!string.IsNullOrWhiteSpace(roomType))
        {
            activeRoomsQuery = activeRoomsQuery.Where(r => r.RoomType == roomType);
        }

        if (minCapacity.HasValue)
        {
            activeRoomsQuery = activeRoomsQuery.Where(r => r.Capacity >= minCapacity.Value);
        }

        // 3. Get conflicting room IDs from ScheduledSlots
        var conflictingRoomIds = await _context.ScheduledSlots
            .Include(ss => ss.Timetable)
            .Where(ss => ss.Timetable.TermId == termId &&
                        ss.DayOfWeek == dayOfWeek &&
                        ss.StartTime < endTime &&
                        ss.EndTime > startTime &&
                        ss.RoomId.HasValue)
            .Select(ss => ss.RoomId!.Value)
            .Distinct()
            .ToListAsync();

        // 4. Exclude conflicting rooms and return ordered by capacity
        return await activeRoomsQuery
            .Where(r => !conflictingRoomIds.Contains(r.Id))
            .OrderBy(r => r.Capacity)
            .ToListAsync();
    }

    public async Task<Room?> GetRoomWithScheduleAsync(int roomId, int termId)
    {
        return await _dbSet
            .Include(r => r.ScheduledSlots.Where(ss => ss.Timetable.TermId == termId))
                .ThenInclude(ss => ss.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(r => r.ScheduledSlots.Where(ss => ss.Timetable.TermId == termId))
                .ThenInclude(ss => ss.Timetable)
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }
}
