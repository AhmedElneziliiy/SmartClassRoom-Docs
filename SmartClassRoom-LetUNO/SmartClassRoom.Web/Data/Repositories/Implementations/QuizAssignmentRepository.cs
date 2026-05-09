using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Quizzes;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class QuizAssignmentRepository : Repository<QuizAssignment>, IQuizAssignmentRepository
{
    public QuizAssignmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<QuizAssignment>> GetAssignmentsByCourseOfferingAsync(int courseOfferingId)
    {
        return await _dbSet
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
            .Include(a => a.CourseOffering)
            .Where(a => a.CourseOfferingId == courseOfferingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<QuizAssignment>> GetAvailableAssignmentsForStudentAsync(int studentId, DateTime currentTime)
    {
        // Get student's enrollments to find their course offerings
        var studentEnrollments = await _context.Set<SmartClassRoom.Web.Models.Entities.Academic.StudentEnrollment>()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseOfferingId)
            .ToListAsync();

        return await _dbSet
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
            .Include(a => a.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(a => studentEnrollments.Contains(a.CourseOfferingId) &&
                       a.IsPublished &&
                       a.AvailableFrom <= currentTime &&
                       a.AvailableUntil >= currentTime)
            .OrderBy(a => a.AvailableUntil)
            .ToListAsync();
    }

    public async Task<QuizAssignment?> GetAssignmentWithQuizAsync(int assignmentId)
    {
        return await _dbSet
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions.OrderBy(qq => qq.OrderIndex))
            .Include(a => a.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(a => a.CourseOffering)
                .ThenInclude(co => co.Term)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
    }

    public async Task<bool> IsAssignmentActiveAsync(int assignmentId, DateTime currentTime)
    {
        return await _dbSet
            .AnyAsync(a => a.Id == assignmentId &&
                          a.IsPublished &&
                          a.AvailableFrom <= currentTime &&
                          a.AvailableUntil >= currentTime);
    }
}
