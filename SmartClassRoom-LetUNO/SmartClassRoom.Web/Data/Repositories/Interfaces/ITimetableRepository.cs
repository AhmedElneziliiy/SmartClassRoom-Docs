using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface ITimetableRepository : IRepository<Timetable>
{
    Task<Timetable?> GetByIdWithDetailsAsync(int id);
    Task<Timetable?> GetByIdWithSlotsAsync(int id);
    Task<IEnumerable<Timetable>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Timetable>> GetByTermAsync(int termId);
    Task<Timetable?> GetBySectionAndTermAsync(int sectionId, int termId);
    Task<IEnumerable<ScheduledSlot>> GetAllSlotsForTimetableAsync(int timetableId);
    Task<bool> HasPublishedTimetableAsync(int sectionId, int termId);
    Task<IEnumerable<Timetable>> GetDraftTimetablesAsync();
    Task<IEnumerable<Timetable>> GetPublishedTimetablesAsync(int termId);
}
