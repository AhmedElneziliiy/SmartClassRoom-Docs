using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Department entity with domain-specific queries
/// </summary>
public interface IDepartmentRepository : IRepository<Department>
{
    /// <summary>
    /// Get department with head of department details
    /// </summary>
    Task<Department?> GetWithHeadAsync(int departmentId);

    /// <summary>
    /// Get departments by university
    /// </summary>
    Task<IEnumerable<Department>> GetByUniversityAsync(int universityId);

    /// <summary>
    /// Get active departments only
    /// </summary>
    Task<IEnumerable<Department>> GetActiveDepartmentsAsync();

    /// <summary>
    /// Get active departments by university
    /// </summary>
    Task<IEnumerable<Department>> GetActiveDepartmentsByUniversityAsync(int universityId);

    /// <summary>
    /// Get department with all related data (students, teachers, courses)
    /// </summary>
    Task<Department?> GetDepartmentWithDetailsAsync(int departmentId);

    /// <summary>
    /// Get department by code
    /// </summary>
    Task<Department?> GetByCodeAsync(string code);

    /// <summary>
    /// Get departments statistics (student count, teacher count, course count)
    /// </summary>
    Task<DepartmentStatistics> GetDepartmentStatisticsAsync(int departmentId);
}

/// <summary>
/// DTO for department statistics
/// </summary>
public class DepartmentStatistics
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int CourseCount { get; set; }
    public int ActiveCourseOfferingsCount { get; set; }
}
