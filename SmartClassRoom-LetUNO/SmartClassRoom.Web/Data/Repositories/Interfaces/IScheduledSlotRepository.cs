using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IScheduledSlotRepository : IRepository<ScheduledSlot>
{
    Task<IEnumerable<ScheduledSlot>> GetByTimetableAsync(int timetableId);
    Task<IEnumerable<ScheduledSlot>> GetByCourseOfferingAsync(int courseOfferingId);
    Task<IEnumerable<ScheduledSlot>> GetTeacherSlotsAsync(int teacherId, int termId);
    Task<IEnumerable<ScheduledSlot>> GetSectionSlotsAsync(int sectionId, int termId);
    Task<IEnumerable<ScheduledSlot>> GetRoomSlotsAsync(int roomId, int termId);

    Task<ScheduledSlot?> FindSlotAsync(string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int timetableId);

    /// <summary>
    /// Check if teacher has a conflict at the specified time
    /// </summary>
    Task<bool> HasTeacherConflictAsync(int teacherId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int termId, int? excludeSlotId = null);

    /// <summary>
    /// Check if section has a conflict at the specified time
    /// </summary>
    Task<bool> HasSectionConflictAsync(int sectionId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int termId, int? excludeSlotId = null);

    /// <summary>
    /// Check if room has a conflict at the specified time
    /// </summary>
    Task<bool> HasRoomConflictAsync(int roomId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int termId, int? excludeSlotId = null);
}
