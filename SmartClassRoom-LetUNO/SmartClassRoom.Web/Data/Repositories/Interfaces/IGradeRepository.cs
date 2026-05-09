using SmartClassRoom.Web.Models.Entities.Grading;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IGradeRepository : IRepository<Grade>
{
    // Get all grades for a specific student in a course offering
    Task<IEnumerable<Grade>> GetGradesByStudentAndOfferingAsync(int studentId, int courseOfferingId);

    // Get all grades for a specific grade component
    Task<IEnumerable<Grade>> GetGradesByComponentAsync(int gradeComponentId);

    // Get grade for specific student, offering, and component
    Task<Grade?> GetGradeAsync(int studentId, int courseOfferingId, int gradeComponentId);

    // Get all grades for an offering (for grade sheet)
    Task<IEnumerable<Grade>> GetGradesByOfferingAsync(int courseOfferingId);

    // Check if grade exists
    Task<bool> GradeExistsAsync(int studentId, int courseOfferingId, int gradeComponentId);
}
