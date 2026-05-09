using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IQuizQuestionRepository : IRepository<QuizQuestion>
{
    Task<IEnumerable<QuizQuestion>> GetQuestionsByQuizAsync(int quizId);
    Task<int> GetQuestionCountAsync(int quizId);
    Task<decimal> GetTotalPointsAsync(int quizId);
}
