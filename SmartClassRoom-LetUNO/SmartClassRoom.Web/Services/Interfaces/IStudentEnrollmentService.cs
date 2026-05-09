using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Enrollments;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface IStudentEnrollmentService
{
    Task<(bool Success, string Message)> EnrollStudentAsync(int studentId, int courseOfferingId);
    Task<(bool Success, string Message)> DropCourseAsync(int enrollmentId);
    Task<IEnumerable<StudentEnrollment>> GetStudentEnrollmentsAsync(int studentId, int? termId = null);
    Task<IEnumerable<StudentEnrollment>> GetOfferingEnrollmentsAsync(int offeringId);
    Task<(bool IsEligible, string Message)> ValidateEnrollmentAsync(int studentId, int courseOfferingId);

    /// <summary>
    /// Auto-enroll students based on course offering criteria (section or department matching)
    /// </summary>
    Task<(int SuccessCount, int FailureCount, List<string> Errors)> AutoEnrollStudentsAsync(int courseOfferingId);

    /// <summary>
    /// Bulk enroll specific students by their IDs
    /// </summary>
    Task<(int SuccessCount, int FailureCount, List<string> Errors)> EnrollStudentsByIdsAsync(int courseOfferingId, List<int> studentIds);

    /// <summary>
    /// Unenroll a student from a course offering
    /// </summary>
    Task<(bool Success, string Message)> UnenrollStudentAsync(int enrollmentId);

    /// <summary>
    /// Get students eligible for enrollment in a course offering (for manual enrollment UI)
    /// </summary>
    Task<IEnumerable<StudentEligibilityDto>> GetEligibleStudentsAsync(int courseOfferingId, string? searchTerm = null);
}
