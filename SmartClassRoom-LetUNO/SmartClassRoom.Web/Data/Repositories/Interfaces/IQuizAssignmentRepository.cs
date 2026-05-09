using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IQuizAssignmentRepository : IRepository<QuizAssignment>
{
    Task<IEnumerable<QuizAssignment>> GetAssignmentsByCourseOfferingAsync(int courseOfferingId);
    Task<IEnumerable<QuizAssignment>> GetAvailableAssignmentsForStudentAsync(int studentId, DateTime currentTime);
    Task<QuizAssignment?> GetAssignmentWithQuizAsync(int assignmentId);
    Task<bool> IsAssignmentActiveAsync(int assignmentId, DateTime currentTime);
}
