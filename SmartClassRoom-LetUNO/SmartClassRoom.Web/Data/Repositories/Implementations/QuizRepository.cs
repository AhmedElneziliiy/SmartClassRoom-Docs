using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class QuizRepository : Repository<Quiz>, IQuizRepository
{
    public QuizRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
    {
        return await _dbSet
            .Include(q => q.Questions.OrderBy(qq => qq.OrderIndex))
            .Include(q => q.Course)
            .Include(q => q.Creator)
            .FirstOrDefaultAsync(q => q.Id == quizId);
    }

    public async Task<Quiz?> GetQuizWithAssignmentsAsync(int quizId)
    {
        return await _dbSet
            .Include(q => q.Assignments)
                .ThenInclude(a => a.CourseOffering)
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.Id == quizId);
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByCourseAsync(int courseId)
    {
        return await _dbSet
            .Include(q => q.Course)
            .Include(q => q.Creator)
            .Include(q => q.Questions)
            .Where(q => q.CourseId == courseId && q.IsActive)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByTeacherAsync(int teacherId)
    {
        return await _dbSet
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .Where(q => q.CreatorId == teacherId && q.IsActive)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasActiveAssignmentsAsync(int quizId)
    {
        return await _context.Set<QuizAssignment>()
            .AnyAsync(a => a.QuizId == quizId && a.IsPublished);
    }
}
