using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Student entity with domain-specific queries
/// </summary>
public interface IStudentRepository : IRepository<Student>
{
    /// <summary>
    /// Get students by department with navigation properties loaded
    /// </summary>
    Task<IEnumerable<Student>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get students by level
    /// </summary>
    Task<IEnumerable<Student>> GetByLevelAsync(int levelId);

    /// <summary>
    /// Get students by section
    /// </summary>
    Task<IEnumerable<Student>> GetBySectionAsync(int sectionId);

    /// <summary>
    /// Get students by group
    /// </summary>
    Task<IEnumerable<Student>> GetByGroupAsync(int groupId);

    /// <summary>
    /// Get active students only
    /// </summary>
    Task<IEnumerable<Student>> GetActiveStudentsAsync();

    /// <summary>
    /// Get active students by department
    /// </summary>
    Task<IEnumerable<Student>> GetActiveStudentsByDepartmentAsync(int departmentId);

    /// <summary>
    /// Search students by name or student code
    /// </summary>
    /// <param name="searchTerm">Search term for name or code</param>
    Task<IEnumerable<Student>> SearchByNameOrCodeAsync(string searchTerm);

    /// <summary>
    /// Get student with all related data (department, level, section, group)
    /// </summary>
    Task<Student?> GetStudentWithDetailsAsync(int studentId);

    /// <summary>
    /// Get students enrolled in a specific course offering
    /// </summary>
    Task<IEnumerable<Student>> GetStudentsByCourseOfferingAsync(int courseOfferingId);

    /// <summary>
    /// Get student by student code
    /// </summary>
    Task<Student?> GetByStudentCodeAsync(string studentCode);
}
