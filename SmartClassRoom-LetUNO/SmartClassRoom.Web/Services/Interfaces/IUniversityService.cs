using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.ViewModels.Universities;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for managing University entities
/// </summary>
public interface IUniversityService
{
    /// <summary>
    /// Get paginated list of universities with filtering and sorting
    /// </summary>
    Task<PagedResult<University>> GetUniversitiesPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Get university by ID
    /// </summary>
    Task<University?> GetUniversityByIdAsync(int id);

    /// <summary>
    /// Get university with all departments included
    /// </summary>
    Task<University?> GetUniversityWithDepartmentsAsync(int id);

    /// <summary>
    /// Check if university code exists (case-sensitive)
    /// </summary>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Create a new university
    /// </summary>
    Task<(bool Success, string Message)> CreateUniversityAsync(CreateUniversityRequest request);

    /// <summary>
    /// Update an existing university
    /// </summary>
    Task<(bool Success, string Message)> UpdateUniversityAsync(EditUniversityRequest request);

    /// <summary>
    /// Deactivate university (soft delete with validation)
    /// </summary>
    Task<(bool Success, string Message)> DeactivateUniversityAsync(int id);

    /// <summary>
    /// Activate a deactivated university
    /// </summary>
    Task<(bool Success, string Message)> ActivateUniversityAsync(int id);

    /// <summary>
    /// Get university statistics (department counts, student/teacher counts)
    /// </summary>
    Task<UniversityStatistics> GetUniversityStatisticsAsync(int id);
}

/// <summary>
/// Statistics model for University details view
/// </summary>
public class UniversityStatistics
{
    public int UniversityId { get; set; }
    public string UniversityName { get; set; } = string.Empty;
    public int DepartmentCount { get; set; }
    public int ActiveDepartmentCount { get; set; }
    public int TotalStudentCount { get; set; }
    public int TotalTeacherCount { get; set; }
}
