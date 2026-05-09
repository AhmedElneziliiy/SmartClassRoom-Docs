using Microsoft.AspNetCore.Identity;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.ViewModels.Users;
using SmartClassRoom.Web.Models.DTOs.Request;
using SmartClassRoom.Web.Models.Entities.Identity;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Get all users with optional filtering
    /// </summary>
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(string? role = null, string? userType = null, bool? isActive = null);

    /// <summary>
    /// Get users with pagination and optional filtering/sorting
    /// </summary>
    /// <param name="pageNumber">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="role">Filter by role (optional)</param>
    /// <param name="userType">Filter by user type (optional)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="sortBy">Sort column: Name, Email, UserType, Status, Created (optional)</param>
    /// <param name="ascending">Sort direction (default: true)</param>
    Task<PagedResult<ApplicationUser>> GetUsersPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? role = null,
        string? userType = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<ApplicationUser?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    Task<IdentityResult> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// Update existing user
    /// </summary>
    Task<IdentityResult> UpdateUserAsync(EditUserRequest request);

    /// <summary>
    /// Deactivate user (soft delete)
    /// </summary>
    Task<IdentityResult> DeactivateUserAsync(int userId);

    /// <summary>
    /// Activate user
    /// </summary>
    Task<IdentityResult> ActivateUserAsync(int userId);

    /// <summary>
    /// Reset user password (Admin only)
    /// </summary>
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>
    /// Get user roles
    /// </summary>
    Task<IList<string>> GetUserRolesAsync(int userId);

    /// <summary>
    /// Update user role
    /// </summary>
    Task<IdentityResult> UpdateUserRoleAsync(int userId, string newRole);

    /// <summary>
    /// Check if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);

    /// <summary>
    /// Reset user's UDID (Admin only)
    /// </summary>
    Task<IdentityResult> ResetUDIDAsync(int userId);

    /// <summary>
    /// Get all roles for dropdown
    /// </summary>
    Task<IEnumerable<string>> GetAllRolesAsync();
}
