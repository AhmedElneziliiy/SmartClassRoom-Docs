using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class QuizAttemptRepository : Repository<QuizAttempt>, IQuizAttemptRepository
{
    public QuizAttemptRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<QuizAttempt>> GetAttemptsByStudentAsync(int studentId, int assignmentId)
    {
        return await _dbSet
            .Include(a => a.QuizAssignment)
                .ThenInclude(qa => qa.Quiz)
            .Where(a => a.StudentId == studentId && a.QuizAssignmentId == assignmentId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();
    }

    public async Task<int> GetAttemptCountAsync(int studentId, int assignmentId)
    {
        return await _dbSet
            .Where(a => a.StudentId == studentId && a.QuizAssignmentId == assignmentId)
            .CountAsync();
    }

    public async Task<QuizAttempt?> GetAttemptWithDetailsAsync(int attemptId)
    {
        return await _dbSet
            .Include(a => a.QuizAssignment)
                .ThenInclude(qa => qa.Quiz)
                    .ThenInclude(q => q.Questions.OrderBy(qq => qq.OrderIndex))
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.Id == attemptId);
    }

    public async Task<IEnumerable<QuizAttempt>> GetAttemptsByAssignmentAsync(int assignmentId)
    {
        return await _dbSet
            .Include(a => a.Student)
            .Include(a => a.QuizAssignment)
                .ThenInclude(qa => qa.Quiz)
            .Where(a => a.QuizAssignmentId == assignmentId)
            .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
            .ToListAsync();
    }

    public async Task<QuizAttempt?> GetInProgressAttemptAsync(int studentId, int assignmentId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.StudentId == studentId &&
                                     a.QuizAssignmentId == assignmentId &&
                                     a.Status == "InProgress");
    }
}
