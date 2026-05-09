using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IStudentEnrollmentRepository : IRepository<StudentEnrollment>
{
    Task<StudentEnrollment?> GetEnrollmentAsync(int studentId, int offeringId);
    Task<IEnumerable<StudentEnrollment>> GetStudentEnrollmentsAsync(int studentId, int? termId = null);
    Task<IEnumerable<StudentEnrollment>> GetOfferingEnrollmentsAsync(int offeringId);
    Task<int> GetOfferingEnrollmentCountAsync(int offeringId);
    Task<bool> IsStudentEnrolledAsync(int studentId, int offeringId);
    Task<IEnumerable<StudentEnrollment>> GetCompletedCoursesAsync(int studentId);
}
