using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Courses;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for Course business logic
/// </summary>
public interface ICourseService
{
    /// <summary>
    /// Get paginated list of courses with filtering and sorting
    /// </summary>
    Task<PagedResult<Course>> GetCoursesPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Get course by ID
    /// </summary>
    Task<Course?> GetCourseByIdAsync(int id);

    /// <summary>
    /// Get course with all details (Department, Prerequisite, Offerings)
    /// </summary>
    Task<Course?> GetCourseWithDetailsAsync(int id);

    /// <summary>
    /// Check if course code exists (excluding a specific course for edit)
    /// </summary>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Create a new course
    /// </summary>
    Task<(bool Success, string Message)> CreateCourseAsync(CreateCourseRequest request);

    /// <summary>
    /// Update an existing course
    /// </summary>
    Task<(bool Success, string Message)> UpdateCourseAsync(EditCourseRequest request);

    /// <summary>
    /// Deactivate a course (soft delete)
    /// </summary>
    Task<(bool Success, string Message)> DeactivateCourseAsync(int id);

    /// <summary>
    /// Activate a course
    /// </summary>
    Task<(bool Success, string Message)> ActivateCourseAsync(int id);

    /// <summary>
    /// Get course statistics
    /// </summary>
    Task<CourseStatistics> GetCourseStatisticsAsync(int id);

    /// <summary>
    /// Get prerequisite courses for a given course
    /// </summary>
    Task<IEnumerable<Course>> GetPrerequisitesAsync(int courseId);

    /// <summary>
    /// Validate if a student has completed all prerequisites for a course
    /// </summary>
    Task<(bool IsEligible, string Message)> ValidatePrerequisitesAsync(int studentId, int courseId);
}

/// <summary>
/// Course statistics model
/// </summary>
public class CourseStatistics
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int OfferingsCount { get; set; }
    public int ActiveOfferingsCount { get; set; }
    public int TotalEnrolledStudents { get; set; }
    public bool HasPrerequisite { get; set; }
    public string? PrerequisiteCourseName { get; set; }
}
