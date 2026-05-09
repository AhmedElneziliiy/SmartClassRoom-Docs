using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.ViewModels.Users;
using SmartClassRoom.Web.Models.DTOs.Request;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(string? role = null, string? userType = null, bool? isActive = null)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(userType))
        {
            query = query.Where(u => u.UserType == userType);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var users = await query.ToListAsync();

        // Filter by role if specified
        if (!string.IsNullOrEmpty(role))
        {
            var usersInRole = new List<ApplicationUser>();
            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains(role))
                {
                    usersInRole.Add(user);
                }
            }
            return usersInRole;
        }

        return users;
    }

    public async Task<PagedResult<ApplicationUser>> GetUsersPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? role = null,
        string? userType = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Validate page number
        if (pageNumber < 1) pageNumber = 1;

        // Step 1: Apply database-level filters (UserType and IsActive)
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(userType))
        {
            query = query.Where(u => u.UserType == userType);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        // Load filtered users into memory for role filtering
        var allFilteredUsers = await query.ToListAsync();

        // Step 2: Filter by role if specified (requires in-memory processing due to UserManager)
        IEnumerable<ApplicationUser> users = allFilteredUsers;

        if (!string.IsNullOrEmpty(role))
        {
            var usersInRole = new List<ApplicationUser>();
            foreach (var user in allFilteredUsers)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains(role))
                {
                    usersInRole.Add(user);
                }
            }
            users = usersInRole;
        }

        // Step 3: Apply sorting
        users = sortBy?.ToLower() switch
        {
            "name" => ascending ? users.OrderBy(u => u.FullName) : users.OrderByDescending(u => u.FullName),
            "email" => ascending ? users.OrderBy(u => u.Email) : users.OrderByDescending(u => u.Email),
            "usertype" => ascending ? users.OrderBy(u => u.UserType) : users.OrderByDescending(u => u.UserType),
            "status" => ascending ? users.OrderBy(u => u.IsActive) : users.OrderByDescending(u => u.IsActive),
            "created" => ascending ? users.OrderBy(u => u.CreatedAt) : users.OrderByDescending(u => u.CreatedAt),
            _ => users.OrderBy(u => u.FullName) // Default sort by name
        };

        // Step 4: Get total count before pagination
        int totalCount = users.Count();

        // Step 5: Apply pagination
        var pagedUsers = users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Step 6: Return PagedResult
        return PagedResult<ApplicationUser>.Create(pagedUsers, totalCount, pageNumber, pageSize);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(int userId)
    {
        return await _userManager.Users
            .Include(u => (u as Student).University)
            .Include(u => (u as Student).Department)
            .Include(u => (u as Student).Level)
            .Include(u => (u as Student).Section)
            .Include(u => (u as Student).Group)
            .Include(u => (u as Teacher).University)
            .Include(u => (u as Teacher).Department)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IdentityResult> CreateUserAsync(CreateUserRequest request)
    {
        // Check if email already exists
        if (await EmailExistsAsync(request.Email))
        {
            return IdentityResult.Failed(new IdentityError { Description = "User with this email already exists" });
        }

        // Create user based on type
        ApplicationUser newUser;

        if (request.UserType == "Teacher")
        {
            newUser = new Teacher
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                NationalId = request.NationalId,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                UserType = "Teacher",
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UniversityId = request.UniversityId,
                EmployeeCode = request.EmployeeCode,
                DepartmentId = request.DepartmentId,
                Title = request.Title,
                Specialization = request.Specialization,
                HireDate = request.HireDate,
                EmploymentStatus = request.EmploymentStatus
            };
        }
        else // Student
        {
            newUser = new Student
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                NationalId = request.NationalId,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                UserType = "Student",
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UniversityId = request.UniversityId,
                StudentCode = request.StudentCode,
                DepartmentId = request.DepartmentId,
                LevelId = request.LevelId,
                SectionId = request.SectionId,
                GroupId = request.GroupId,
                EnrollmentDate = request.EnrollmentDate,
                AcademicStatus = request.AcademicStatus
            };
        }

        // Use default password if not provided
        const string defaultPassword = "Pass@123";
        var password = string.IsNullOrWhiteSpace(request.Password) ? defaultPassword : request.Password;

        var result = await _userManager.CreateAsync(newUser, password);

        if (result.Succeeded)
        {
            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = request.Role,
                    Description = $"{request.Role} role",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Assign role
            await _userManager.AddToRoleAsync(newUser, request.Role);
        }

        return result;
    }

    public async Task<IdentityResult> UpdateUserAsync(EditUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        // Check if email is being changed and if it already exists
        if (user.Email != request.Email && await EmailExistsAsync(request.Email, request.UserId))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Email is already in use" });
        }

        // Update common properties
        user.FullName = request.FullName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.NationalId = request.NationalId;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;
        user.Address = request.Address;
        user.IsActive = request.IsActive;
        user.UniversityId = request.UniversityId;
        user.UpdatedAt = DateTime.UtcNow;

        // Update type-specific properties
        if (user is Teacher teacher)
        {
            teacher.EmployeeCode = request.EmployeeCode;
            teacher.DepartmentId = request.DepartmentId;
            teacher.Title = request.Title;
            teacher.Specialization = request.Specialization;
            teacher.HireDate = request.HireDate;
            teacher.EmploymentStatus = request.EmploymentStatus;
        }
        else if (user is Student student)
        {
            student.StudentCode = request.StudentCode;
            student.DepartmentId = request.DepartmentId;
            student.LevelId = request.LevelId;
            student.SectionId = request.SectionId;
            student.GroupId = request.GroupId;
            student.EnrollmentDate = request.EnrollmentDate;
            student.AcademicStatus = request.AcademicStatus;
            student.CurrentGPA = request.CurrentGPA;
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // Update role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(request.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Ensure role exists
                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    await _roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = request.Role,
                        Description = $"{request.Role} role",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _userManager.AddToRoleAsync(user, request.Role);
            }
        }

        return result;
    }

    public async Task<IdentityResult> DeactivateUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> ActivateUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        // Remove old password and set new one
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (result.Succeeded)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return result;
    }

    public async Task<IList<string>> GetUserRolesAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new List<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> UpdateUserRoleAsync(int userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Ensure role exists
        if (!await _roleManager.RoleExistsAsync(newRole))
        {
            await _roleManager.CreateAsync(new ApplicationRole
            {
                Name = newRole,
                Description = $"{newRole} role",
                CreatedAt = DateTime.UtcNow
            });
        }

        return await _userManager.AddToRoleAsync(user, newRole);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return false;
        }

        if (excludeUserId.HasValue && user.Id == excludeUserId.Value)
        {
            return false;
        }

        return true;
    }

    public async Task<IdentityResult> ResetUDIDAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        user.UDID = null;
        user.UpdatedAt = DateTime.UtcNow;

        return await _userManager.UpdateAsync(user);
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
    }
}
