using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.ViewModels.Timetables;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface ITimetableService
{
    // Generation
    Task<TimetableGenerationResult> GenerateTimetableAsync(GenerateTimetableRequest request, int? userId = null);

    // Retrieval
    Task<Timetable?> GetTimetableByIdAsync(int id);
    Task<Timetable?> GetTimetableWithSlotsAsync(int id);
    Task<IEnumerable<ScheduledSlotDto>> GetTimetableSlotsAsync(int id);

    // Conflict Detection
    Task<List<ConflictInfo>> DetectConflictsAsync(int timetableId);

    // Manual Editing
    Task<(bool Success, string Message)> UpdateSlotAsync(UpdateSlotRequest request);
    Task<(bool Success, string Message)> DeleteSlotAsync(int slotId);
    Task<(bool Success, string Message)> AddSlotAsync(int timetableId, int courseOfferingId, TimeSlotDto timeSlot, int? roomId);
    Task<(bool Success, string Message)> SwapSlotsAsync(SwapSlotsRequest request);

    // Publishing
    Task<PublishTimetableResult> PublishTimetableAsync(int timetableId, int? userId = null);

    // Management
    Task<(bool Success, string Message)> DeleteTimetableAsync(int id);
}
