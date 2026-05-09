using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.ViewModels.Departments;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for managing Department entities
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Get paginated list of departments with filtering and sorting
    /// </summary>
    Task<PagedResult<Department>> GetDepartmentsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? universityId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Get department by ID
    /// </summary>
    Task<Department?> GetDepartmentByIdAsync(int id);

    /// <summary>
    /// Get department with all details (University, HeadOfDepartment, Students, Teachers, Courses)
    /// </summary>
    Task<Department?> GetDepartmentWithDetailsAsync(int id);

    /// <summary>
    /// Get available teachers for head of department assignment
    /// </summary>
    Task<IEnumerable<Teacher>> GetAvailableHeadsOfDepartmentAsync(int departmentId, int? currentHeadId = null);

    /// <summary>
    /// Check if department code exists (case-sensitive)
    /// </summary>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Create a new department
    /// </summary>
    Task<(bool Success, string Message)> CreateDepartmentAsync(CreateDepartmentRequest request);

    /// <summary>
    /// Update an existing department
    /// </summary>
    Task<(bool Success, string Message)> UpdateDepartmentAsync(EditDepartmentRequest request);

    /// <summary>
    /// Deactivate department (soft delete with validation)
    /// </summary>
    Task<(bool Success, string Message)> DeactivateDepartmentAsync(int id);

    /// <summary>
    /// Activate a deactivated department
    /// </summary>
    Task<(bool Success, string Message)> ActivateDepartmentAsync(int id);

    /// <summary>
    /// Assign or remove head of department
    /// </summary>
    Task<(bool Success, string Message)> AssignHeadOfDepartmentAsync(int departmentId, int? teacherId);

    /// <summary>
    /// Get department statistics using existing repository method
    /// </summary>
    Task<DepartmentStatistics> GetDepartmentStatisticsAsync(int id);
}
