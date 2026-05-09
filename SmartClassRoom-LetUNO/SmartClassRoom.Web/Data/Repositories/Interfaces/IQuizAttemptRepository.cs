using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IQuizAttemptRepository : IRepository<QuizAttempt>
{
    Task<IEnumerable<QuizAttempt>> GetAttemptsByStudentAsync(int studentId, int assignmentId);
    Task<int> GetAttemptCountAsync(int studentId, int assignmentId);
    Task<QuizAttempt?> GetAttemptWithDetailsAsync(int attemptId);
    Task<IEnumerable<QuizAttempt>> GetAttemptsByAssignmentAsync(int assignmentId);
    Task<QuizAttempt?> GetInProgressAttemptAsync(int studentId, int assignmentId);
}
