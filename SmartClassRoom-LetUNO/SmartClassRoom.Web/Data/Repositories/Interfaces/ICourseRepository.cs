using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Course entity with domain-specific queries
/// </summary>
public interface ICourseRepository : IRepository<Course>
{
    /// <summary>
    /// Get courses by department
    /// </summary>
    Task<IEnumerable<Course>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get course with prerequisite information
    /// </summary>
    Task<Course?> GetWithPrerequisitesAsync(int courseId);

    /// <summary>
    /// Get courses that have a specific course as prerequisite
    /// </summary>
    Task<IEnumerable<Course>> GetCoursesRequiringPrerequisiteAsync(int prerequisiteCourseId);

    /// <summary>
    /// Search courses by name or code
    /// </summary>
    Task<IEnumerable<Course>> SearchByNameOrCodeAsync(string searchTerm);

    /// <summary>
    /// Get course by code
    /// </summary>
    Task<Course?> GetByCourseCodeAsync(string courseCode);

    /// <summary>
    /// Get active courses only
    /// </summary>
    Task<IEnumerable<Course>> GetActiveCoursesAsync();

    /// <summary>
    /// Get active courses by department
    /// </summary>
    Task<IEnumerable<Course>> GetActiveCoursesByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get courses by type (Lecture, Lab, Hybrid)
    /// </summary>
    Task<IEnumerable<Course>> GetByCourseTypeAsync(string courseType);

    /// <summary>
    /// Get course with all offerings
    /// </summary>
    Task<Course?> GetCourseWithOfferingsAsync(int courseId);
}
