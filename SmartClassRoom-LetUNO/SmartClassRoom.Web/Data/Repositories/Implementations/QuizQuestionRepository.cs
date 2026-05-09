using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class QuizQuestionRepository : Repository<QuizQuestion>, IQuizQuestionRepository
{
    public QuizQuestionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<QuizQuestion>> GetQuestionsByQuizAsync(int quizId)
    {
        return await _dbSet
            .Where(qq => qq.QuizId == quizId)
            .OrderBy(qq => qq.OrderIndex)
            .ToListAsync();
    }

    public async Task<int> GetQuestionCountAsync(int quizId)
    {
        return await _dbSet
            .Where(qq => qq.QuizId == quizId)
            .CountAsync();
    }

    public async Task<decimal> GetTotalPointsAsync(int quizId)
    {
        return await _dbSet
            .Where(qq => qq.QuizId == quizId)
            .SumAsync(qq => qq.Points);
    }
}
