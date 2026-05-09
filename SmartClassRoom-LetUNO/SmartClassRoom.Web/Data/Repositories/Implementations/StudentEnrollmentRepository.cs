using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class StudentEnrollmentRepository : Repository<StudentEnrollment>, IStudentEnrollmentRepository
{
    public StudentEnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<StudentEnrollment?> GetEnrollmentAsync(int studentId, int offeringId)
    {
        // Get all matching enrollments
        var enrollments = await _dbSet
            .Include(e => e.Student)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Term)
            .Where(e => e.StudentId == studentId && e.CourseOfferingId == offeringId)
            .ToListAsync();

        // If no enrollments, return null
        if (!enrollments.Any()) return null;

        // Priority order: Enrolled > Completed > Dropped
        // Return the most recent 'Enrolled' enrollment if exists
        var enrolledRecord = enrollments
            .Where(e => e.Status == "Enrolled")
            .OrderByDescending(e => e.EnrollmentDate)
            .FirstOrDefault();

        if (enrolledRecord != null) return enrolledRecord;

        // Otherwise return the most recent enrollment (regardless of status)
        return enrollments
            .OrderByDescending(e => e.EnrollmentDate)
            .First();
    }

    public async Task<IEnumerable<StudentEnrollment>> GetStudentEnrollmentsAsync(int studentId, int? termId = null)
    {
        var query = _dbSet
            .Include(e => e.Student)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Term)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Where(e => e.StudentId == studentId);

        if (termId.HasValue)
        {
            query = query.Where(e => e.CourseOffering.TermId == termId.Value);
        }

        return await query
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentEnrollment>> GetOfferingEnrollmentsAsync(int offeringId)
    {
        return await _dbSet
            .Include(e => e.Student)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(e => e.CourseOfferingId == offeringId)
            .OrderBy(e => e.Student.FullName)
            .ToListAsync();
    }

    public async Task<int> GetOfferingEnrollmentCountAsync(int offeringId)
    {
        return await _dbSet
            .Where(e => e.CourseOfferingId == offeringId)
            .Where(e => e.Status == "Enrolled")
            .CountAsync();
    }

    public async Task<bool> IsStudentEnrolledAsync(int studentId, int offeringId)
    {
        return await _dbSet
            .AnyAsync(e => e.StudentId == studentId &&
                          e.CourseOfferingId == offeringId &&
                          e.Status == "Enrolled");
    }

    public async Task<IEnumerable<StudentEnrollment>> GetCompletedCoursesAsync(int studentId)
    {
        return await _dbSet
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(e => e.StudentId == studentId)
            .Where(e => e.Status == "Completed")
            .ToListAsync();
    }
}
