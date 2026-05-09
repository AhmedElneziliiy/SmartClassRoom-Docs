using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IQuizRepository : IRepository<Quiz>
{
    Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
    Task<Quiz?> GetQuizWithAssignmentsAsync(int quizId);
    Task<IEnumerable<Quiz>> GetQuizzesByCourseAsync(int courseId);
    Task<IEnumerable<Quiz>> GetQuizzesByTeacherAsync(int teacherId);
    Task<bool> HasActiveAssignmentsAsync(int quizId);
}
