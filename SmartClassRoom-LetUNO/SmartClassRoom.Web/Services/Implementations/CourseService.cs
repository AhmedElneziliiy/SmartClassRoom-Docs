using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Courses;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICourseRepository _courseRepository;

    public CourseService(IUnitOfWork unitOfWork, ICourseRepository courseRepository)
    {
        _unitOfWork = unitOfWork;
        _courseRepository = courseRepository;
    }

    public async Task<PagedResult<Course>> GetCoursesPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true)
    {
        // Get all courses with department
        var allCourses = await _courseRepository.GetAllAsync("Department", "PrerequisiteCourse");

        // Apply filtering
        IEnumerable<Course> filtered = allCourses;

        if (departmentId.HasValue)
        {
            filtered = filtered.Where(c => c.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            filtered = filtered.Where(c => c.IsActive == isActive.Value);
        }

        // Apply sorting
        IEnumerable<Course> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("name", true) => filtered.OrderBy(c => c.Name),
            ("name", false) => filtered.OrderByDescending(c => c.Name),
            ("code", true) => filtered.OrderBy(c => c.Code),
            ("code", false) => filtered.OrderByDescending(c => c.Code),
            ("credits", true) => filtered.OrderBy(c => c.Credits),
            ("credits", false) => filtered.OrderByDescending(c => c.Credits),
            ("coursetype", true) => filtered.OrderBy(c => c.CourseType),
            ("coursetype", false) => filtered.OrderByDescending(c => c.CourseType),
            ("department", true) => filtered.OrderBy(c => c.Department.Name),
            ("department", false) => filtered.OrderByDescending(c => c.Department.Name),
            ("status", true) => filtered.OrderBy(c => c.IsActive),
            ("status", false) => filtered.OrderByDescending(c => c.IsActive),
            ("createdat", true) => filtered.OrderBy(c => c.CreatedAt),
            ("createdat", false) => filtered.OrderByDescending(c => c.CreatedAt),
            _ => filtered.OrderBy(c => c.Code)
        };

        var totalCount = sorted.Count();
        var items = sorted.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return PagedResult<Course>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _courseRepository.GetByIdAsync(id);
    }

    public async Task<Course?> GetCourseWithDetailsAsync(int id)
    {
        return await _courseRepository.FirstOrDefaultAsync(
            c => c.Id == id,
            "Department",
            "PrerequisiteCourse",
            "Offerings");
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var course = await _courseRepository.GetByCourseCodeAsync(code);

        if (course == null)
            return false;

        if (excludeId.HasValue && course.Id == excludeId.Value)
            return false;

        return true;
    }

    public async Task<(bool Success, string Message)> CreateCourseAsync(CreateCourseRequest request)
    {
        // Validate course code uniqueness
        if (await CodeExistsAsync(request.Code))
        {
            return (false, $"Course code '{request.Code}' already exists.");
        }

        // Validate department exists
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId);
        if (department == null)
        {
            return (false, "Selected department does not exist.");
        }

        // Validate prerequisite course if specified
        if (request.PrerequisiteCourseId.HasValue)
        {
            var prerequisite = await _courseRepository.GetByIdAsync(request.PrerequisiteCourseId.Value);
            if (prerequisite == null)
            {
                return (false, "Selected prerequisite course does not exist.");
            }
        }

        // Create course
        var course = new Course
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Credits = request.Credits,
            CourseType = request.CourseType,
            DepartmentId = request.DepartmentId,
            PrerequisiteCourseId = request.PrerequisiteCourseId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateCourseAsync(EditCourseRequest request)
    {
        var course = await _courseRepository.GetByIdAsync(request.Id);
        if (course == null)
        {
            return (false, "Course not found.");
        }

        // Validate course code uniqueness
        if (await CodeExistsAsync(request.Code, request.Id))
        {
            return (false, $"Course code '{request.Code}' already exists.");
        }

        // Validate department exists
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId);
        if (department == null)
        {
            return (false, "Selected department does not exist.");
        }

        // Validate prerequisite course if specified
        if (request.PrerequisiteCourseId.HasValue)
        {
            var prerequisite = await _courseRepository.GetByIdAsync(request.PrerequisiteCourseId.Value);
            if (prerequisite == null)
            {
                return (false, "Selected prerequisite course does not exist.");
            }

            // Prevent circular prerequisite
            if (request.PrerequisiteCourseId.Value == request.Id)
            {
                return (false, "A course cannot be its own prerequisite.");
            }
        }

        // Update course
        course.Name = request.Name;
        course.Code = request.Code;
        course.Description = request.Description;
        course.Credits = request.Credits;
        course.CourseType = request.CourseType;
        course.DepartmentId = request.DepartmentId;
        course.PrerequisiteCourseId = request.PrerequisiteCourseId;
        course.IsActive = request.IsActive;

        _courseRepository.Update(course);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeactivateCourseAsync(int id)
    {
        var course = await _courseRepository.GetCourseWithOfferingsAsync(id);
        if (course == null)
        {
            return (false, "Course not found.");
        }

        if (!course.IsActive)
        {
            return (false, "Course is already inactive.");
        }

        // Check if course has active offerings
        var hasActiveOfferings = course.Offerings.Any(o => o.Status == "Active");
        if (hasActiveOfferings)
        {
            return (false, "Cannot deactivate course with active offerings. Please deactivate all offerings first.");
        }

        course.IsActive = false;
        _courseRepository.Update(course);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course deactivated successfully.");
    }

    public async Task<(bool Success, string Message)> ActivateCourseAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            return (false, "Course not found.");
        }

        if (course.IsActive)
        {
            return (false, "Course is already active.");
        }

        // Validate department is active
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(course.DepartmentId);
        if (department == null || !department.IsActive)
        {
            return (false, "Cannot activate course because its department is inactive.");
        }

        course.IsActive = true;
        _courseRepository.Update(course);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course activated successfully.");
    }

    public async Task<CourseStatistics> GetCourseStatisticsAsync(int id)
    {
        var course = await _courseRepository.FirstOrDefaultAsync(
            c => c.Id == id,
            "PrerequisiteCourse",
            "Offerings.Enrollments");

        if (course == null)
        {
            return new CourseStatistics { CourseId = id };
        }

        var activeOfferings = course.Offerings.Where(o => o.Status == "Active").ToList();
        var totalEnrolled = course.Offerings.Sum(o => o.Enrollments.Count);

        return new CourseStatistics
        {
            CourseId = course.Id,
            CourseName = course.Name,
            CourseCode = course.Code,
            Credits = course.Credits,
            OfferingsCount = course.Offerings.Count,
            ActiveOfferingsCount = activeOfferings.Count,
            TotalEnrolledStudents = totalEnrolled,
            HasPrerequisite = course.PrerequisiteCourseId.HasValue,
            PrerequisiteCourseName = course.PrerequisiteCourse?.Name
        };
    }

    public async Task<IEnumerable<Course>> GetPrerequisitesAsync(int courseId)
    {
        var course = await _courseRepository.GetWithPrerequisitesAsync(courseId);

        if (course == null || !course.PrerequisiteCourseId.HasValue)
        {
            return Enumerable.Empty<Course>();
        }

        // Return the single prerequisite course
        // Note: Current schema supports only one prerequisite
        // If multiple prerequisites are needed in the future, a junction table would be required
        return course.PrerequisiteCourse != null
            ? new List<Course> { course.PrerequisiteCourse }
            : Enumerable.Empty<Course>();
    }

    public async Task<(bool IsEligible, string Message)> ValidatePrerequisitesAsync(int studentId, int courseId)
    {
        // Get the course with its prerequisites
        var course = await _courseRepository.GetWithPrerequisitesAsync(courseId);

        if (course == null)
        {
            return (false, "Course not found.");
        }

        // If course has no prerequisites, student is eligible
        if (!course.PrerequisiteCourseId.HasValue || course.PrerequisiteCourse == null)
        {
            return (true, "No prerequisites required.");
        }

        // Get student's enrollments to check if they completed the prerequisite
        var studentEnrollments = await _unitOfWork.Repository<StudentEnrollment>()
            .GetAllAsync("CourseOffering", "CourseOffering.Course");

        var prerequisiteCourseId = course.PrerequisiteCourseId.Value;
        var prerequisiteCourseName = course.PrerequisiteCourse.Name;

        // Check if student has completed the prerequisite course
        var completedPrerequisite = studentEnrollments
            .Where(e => e.StudentId == studentId)
            .Where(e => e.CourseOffering.CourseId == prerequisiteCourseId)
            .Where(e => e.Status == "Completed")
            .Any();

        if (!completedPrerequisite)
        {
            return (false, $"Cannot enroll: Prerequisite course '{prerequisiteCourseName}' has not been completed.");
        }

        // Optional: Check if student achieved a minimum grade (e.g., C or above)
        // Uncomment if grade validation is required
        /*
        var prerequisiteEnrollment = studentEnrollments
            .Where(e => e.StudentId == studentId)
            .Where(e => e.CourseOffering.CourseId == prerequisiteCourseId)
            .Where(e => e.Status == "Completed")
            .FirstOrDefault();

        if (prerequisiteEnrollment != null && prerequisiteEnrollment.FinalGrade.HasValue)
        {
            // Assuming passing grade is 60%
            if (prerequisiteEnrollment.FinalGrade.Value < 60)
            {
                return (false, $"Cannot enroll: Minimum passing grade not achieved in prerequisite course '{prerequisiteCourseName}'.");
            }
        }
        */

        return (true, "All prerequisites completed.");
    }
}
