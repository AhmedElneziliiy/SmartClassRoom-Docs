using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Teacher entity with domain-specific queries
/// </summary>
public interface ITeacherRepository : IRepository<Teacher>
{
    /// <summary>
    /// Get teachers by department
    /// </summary>
    Task<IEnumerable<Teacher>> GetByDepartmentAsync(int departmentId);

    /// <summary>
    /// Get active teachers only
    /// </summary>
    Task<IEnumerable<Teacher>> GetActiveTeachersAsync();

    /// <summary>
    /// Get active teachers by department
    /// </summary>
    Task<IEnumerable<Teacher>> GetActiveTeachersByDepartmentAsync(int departmentId);

    /// <summary>
    /// Search teachers by name or employee code
    /// </summary>
    Task<IEnumerable<Teacher>> SearchByNameOrCodeAsync(string searchTerm);

    /// <summary>
    /// Get teacher with department details
    /// </summary>
    Task<Teacher?> GetTeacherWithDetailsAsync(int teacherId);

    /// <summary>
    /// Get teacher by employee code
    /// </summary>
    Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode);

    /// <summary>
    /// Get teachers teaching a specific course
    /// </summary>
    Task<IEnumerable<Teacher>> GetTeachersByCourseAsync(int courseId);

    /// <summary>
    /// Get teachers by employment status
    /// </summary>
    Task<IEnumerable<Teacher>> GetByEmploymentStatusAsync(string employmentStatus);
}
