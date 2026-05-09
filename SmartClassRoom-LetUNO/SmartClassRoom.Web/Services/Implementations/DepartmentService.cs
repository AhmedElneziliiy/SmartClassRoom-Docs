using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.ViewModels.Departments;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Users;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service implementation for managing Department entities
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<Department>> GetDepartmentsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? universityId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Get all departments with related data
        var allDepartments = await _unitOfWork.Departments
            .GetAllAsync("University", "HeadOfDepartment", "Students", "Teachers");

        // Apply filtering
        IEnumerable<Department> filtered = allDepartments;

        if (universityId.HasValue)
        {
            filtered = filtered.Where(d => d.UniversityId == universityId.Value);
        }

        if (isActive.HasValue)
        {
            filtered = filtered.Where(d => d.IsActive == isActive.Value);
        }

        // Apply sorting
        IEnumerable<Department> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("name", true) => filtered.OrderBy(d => d.Name),
            ("name", false) => filtered.OrderByDescending(d => d.Name),
            ("code", true) => filtered.OrderBy(d => d.Code ?? string.Empty),
            ("code", false) => filtered.OrderByDescending(d => d.Code ?? string.Empty),
            ("university", true) => filtered.OrderBy(d => d.University.Name),
            ("university", false) => filtered.OrderByDescending(d => d.University.Name),
            ("status", true) => filtered.OrderBy(d => d.IsActive),
            ("status", false) => filtered.OrderByDescending(d => d.IsActive),
            ("created", true) => filtered.OrderBy(d => d.CreatedAt),
            ("created", false) => filtered.OrderByDescending(d => d.CreatedAt),
            _ => filtered.OrderBy(d => d.Name) // Default sort
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResult<Department>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _unitOfWork.Departments.GetByIdAsync(id);
    }

    public async Task<Department?> GetDepartmentWithDetailsAsync(int id)
    {
        return await _unitOfWork.Departments.GetDepartmentWithDetailsAsync(id);
    }

    public async Task<IEnumerable<Teacher>> GetAvailableHeadsOfDepartmentAsync(int departmentId, int? currentHeadId = null)
    {
        // Get all active teachers from this department
        var teachers = await _unitOfWork.Repository<Teacher>()
            .FindAsync(t => t.DepartmentId == departmentId && t.IsActive);

        return teachers.ToList();
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var existing = await _unitOfWork.Departments.GetByCodeAsync(code);

        if (existing == null)
            return false;

        if (excludeId.HasValue && existing.Id == excludeId.Value)
            return false;

        return true;
    }

    public async Task<(bool Success, string Message)> CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        try
        {
            // Validate university exists
            var university = await _unitOfWork.Repository<University>().GetByIdAsync(request.UniversityId);
            if (university == null)
            {
                return (false, "University not found.");
            }

            // Validate unique code
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                if (await CodeExistsAsync(request.Code))
                {
                    return (false, $"Department code '{request.Code}' already exists.");
                }
            }

            // Validate head of department (if provided)
            if (request.HeadOfDepartmentId.HasValue)
            {
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(request.HeadOfDepartmentId.Value);
                if (teacher == null)
                {
                    return (false, "Head of Department (Teacher) not found.");
                }

                if (!teacher.IsActive)
                {
                    return (false, "Cannot assign inactive teacher as Head of Department.");
                }
            }

            var department = new Department
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                UniversityId = request.UniversityId,
                HeadOfDepartmentId = request.HeadOfDepartmentId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Department '{department.Name}' created successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error creating department: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateDepartmentAsync(EditDepartmentRequest request)
    {
        try
        {
            var department = await GetDepartmentByIdAsync(request.Id);
            if (department == null)
            {
                return (false, "Department not found.");
            }

            // Validate university exists
            var university = await _unitOfWork.Repository<University>().GetByIdAsync(request.UniversityId);
            if (university == null)
            {
                return (false, "University not found.");
            }

            // Validate unique code
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                if (await CodeExistsAsync(request.Code, request.Id))
                {
                    return (false, $"Department code '{request.Code}' already exists.");
                }
            }

            // Validate head of department (if provided)
            if (request.HeadOfDepartmentId.HasValue)
            {
                var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(request.HeadOfDepartmentId.Value);
                if (teacher == null)
                {
                    return (false, "Head of Department (Teacher) not found.");
                }

                if (!teacher.IsActive)
                {
                    return (false, "Cannot assign inactive teacher as Head of Department.");
                }

                if (teacher.DepartmentId != request.Id)
                {
                    return (false, "Head of Department must be a teacher from this department.");
                }
            }

            // Update properties
            department.Name = request.Name;
            department.Code = request.Code;
            department.Description = request.Description;
            department.UniversityId = request.UniversityId;
            department.HeadOfDepartmentId = request.HeadOfDepartmentId;
            department.IsActive = request.IsActive;

            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Department '{department.Name}' updated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating department: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeactivateDepartmentAsync(int id)
    {
        try
        {
            var department = await _unitOfWork.Departments
                .GetByIdAsync(id, "Students", "Teachers", "Courses");

            if (department == null)
            {
                return (false, "Department not found.");
            }

            if (!department.IsActive)
            {
                return (false, "Department is already deactivated.");
            }

            // Validation: Check for active students
            var activeStudents = department.Students.Count(s => s.IsActive);
            if (activeStudents > 0)
            {
                return (false, $"Cannot deactivate department. It has {activeStudents} active student(s).");
            }

            // Validation: Check for active teachers
            var activeTeachers = department.Teachers.Count(t => t.IsActive);
            if (activeTeachers > 0)
            {
                return (false, $"Cannot deactivate department. It has {activeTeachers} active teacher(s).");
            }

            // Validation: Check for any courses
            if (department.Courses.Any())
            {
                return (false, $"Cannot deactivate department. It has {department.Courses.Count} course(s). Please remove or reassign courses first.");
            }

            department.IsActive = false;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Department '{department.Name}' deactivated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error deactivating department: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ActivateDepartmentAsync(int id)
    {
        try
        {
            var department = await _unitOfWork.Departments
                .GetByIdAsync(id, "University");

            if (department == null)
            {
                return (false, "Department not found.");
            }

            if (department.IsActive)
            {
                return (false, "Department is already active.");
            }

            // Validation: Check if parent university is active
            if (!department.University.IsActive)
            {
                return (false, $"Cannot activate department. Parent university '{department.University.Name}' is inactive.");
            }

            department.IsActive = true;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Department '{department.Name}' activated successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error activating department: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> AssignHeadOfDepartmentAsync(int departmentId, int? teacherId)
    {
        try
        {
            var department = await GetDepartmentByIdAsync(departmentId);
            if (department == null)
            {
                return (false, "Department not found.");
            }

            // If removing head (teacherId is null)
            if (!teacherId.HasValue)
            {
                department.HeadOfDepartmentId = null;
                _unitOfWork.Departments.Update(department);
                await _unitOfWork.SaveChangesAsync();
                return (true, "Head of Department removed successfully.");
            }

            // Validate teacher exists
            var teacher = await _unitOfWork.Repository<Teacher>().GetByIdAsync(teacherId.Value);
            if (teacher == null)
            {
                return (false, "Teacher not found.");
            }

            // Validate teacher is active
            if (!teacher.IsActive)
            {
                return (false, "Cannot assign inactive teacher as Head of Department.");
            }

            // Validate teacher belongs to this department
            if (teacher.DepartmentId != departmentId)
            {
                return (false, "Teacher must belong to this department to be assigned as Head.");
            }

            department.HeadOfDepartmentId = teacherId.Value;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Teacher '{teacher.FullName}' assigned as Head of Department successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Error assigning Head of Department: {ex.Message}");
        }
    }

    public async Task<DepartmentStatistics> GetDepartmentStatisticsAsync(int id)
    {
        return await _unitOfWork.Departments.GetDepartmentStatisticsAsync(id);
    }
}
