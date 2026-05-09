using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface ICourseOfferingRepository : IRepository<CourseOffering>
{
    Task<IEnumerable<CourseOffering>> GetByTermAsync(int termId);
    Task<IEnumerable<CourseOffering>> GetByCourseAsync(int courseId);
    Task<IEnumerable<CourseOffering>> GetByTeacherAsync(int teacherId);
    Task<IEnumerable<CourseOffering>> GetBySectionAsync(int sectionId);
    Task<CourseOffering?> GetOfferingWithEnrollmentsAsync(int offeringId);
    Task<CourseOffering?> GetOfferingWithDetailsAsync(int offeringId);
    Task<bool> HasScheduleConflictAsync(int studentId, int offeringId);
    Task<CourseOffering?> GetByIdWithDetailsAsync(int id);
}
