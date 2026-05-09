using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Users;
using SmartClassRoom.Web.Models.ViewModels.Enrollments;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class StudentEnrollmentService : IStudentEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICourseService _courseService;

    public StudentEnrollmentService(IUnitOfWork unitOfWork, ICourseService courseService)
    {
        _unitOfWork = unitOfWork;
        _courseService = courseService;
    }

    public async Task<(bool Success, string Message)> EnrollStudentAsync(int studentId, int courseOfferingId)
    {
        // Validate enrollment eligibility
        var (isEligible, message) = await ValidateEnrollmentAsync(studentId, courseOfferingId);
        if (!isEligible)
        {
            return (false, message);
        }

        // Create enrollment
        var enrollment = new StudentEnrollment
        {
            StudentId = studentId,
            CourseOfferingId = courseOfferingId,
            EnrollmentDate = DateTime.UtcNow,
            Status = "Enrolled"
        };

        await _unitOfWork.StudentEnrollments.AddAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Student enrolled successfully.");
    }

    public async Task<(bool Success, string Message)> DropCourseAsync(int enrollmentId)
    {
        var enrollment = await _unitOfWork.StudentEnrollments.GetByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            return (false, "Enrollment not found.");
        }

        if (enrollment.Status != "Enrolled")
        {
            return (false, $"Cannot drop course. Current status is '{enrollment.Status}'.");
        }

        enrollment.Status = "Dropped";
        _unitOfWork.StudentEnrollments.Update(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course dropped successfully.");
    }

    public async Task<IEnumerable<StudentEnrollment>> GetStudentEnrollmentsAsync(int studentId, int? termId = null)
    {
        return await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(studentId, termId);
    }

    public async Task<IEnumerable<StudentEnrollment>> GetOfferingEnrollmentsAsync(int offeringId)
    {
        return await _unitOfWork.StudentEnrollments.GetOfferingEnrollmentsAsync(offeringId);
    }

    public async Task<(bool IsEligible, string Message)> ValidateEnrollmentAsync(int studentId, int courseOfferingId)
    {
        // 1. Check if student exists and is active
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null)
        {
            return (false, "Student not found.");
        }

        if (!student.IsActive)
        {
            return (false, "Cannot enroll: Student account is not active.");
        }

        // 2. Check if course offering exists and is active
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null)
        {
            return (false, "Course offering not found.");
        }

        if (offering.Status != "Active" && offering.Status != "Upcoming")
        {
            return (false, $"Cannot enroll: Course offering status is '{offering.Status}'.");
        }

        // 3. Check if term is active or upcoming
        var term = offering.Term;
        if (term == null || !term.IsActive)
        {
            return (false, "Cannot enroll: Term is not active.");
        }

        if (term.Status == "Completed")
        {
            return (false, "Cannot enroll: Term has already been completed.");
        }

        // 4. Check if student is already enrolled
        var isAlreadyEnrolled = await _unitOfWork.StudentEnrollments.IsStudentEnrolledAsync(studentId, courseOfferingId);
        if (isAlreadyEnrolled)
        {
            return (false, "Cannot enroll: Student is already enrolled in this course offering.");
        }

        // 5. Check capacity
        var currentEnrollmentCount = await _unitOfWork.StudentEnrollments.GetOfferingEnrollmentCountAsync(courseOfferingId);
        if (currentEnrollmentCount >= offering.MaxStudents)
        {
            return (false, $"Cannot enroll: Course is at capacity ({offering.MaxStudents}/{offering.MaxStudents}).");
        }

        // 6. Check prerequisites
        var course = offering.Course;
        if (course.PrerequisiteCourseId.HasValue)
        {
            var (prerequisiteMet, prerequisiteMessage) = await _courseService.ValidatePrerequisitesAsync(studentId, course.Id);
            if (!prerequisiteMet)
            {
                return (false, prerequisiteMessage);
            }
        }

        // 7. Check schedule conflicts
        var hasConflict = await _unitOfWork.CourseOfferings.HasScheduleConflictAsync(studentId, courseOfferingId);
        if (hasConflict)
        {
            return (false, "Cannot enroll: Schedule conflict with another enrolled course.");
        }

        return (true, "Student is eligible for enrollment.");
    }

    public async Task<(int SuccessCount, int FailureCount, List<string> Errors)> AutoEnrollStudentsAsync(int courseOfferingId)
    {
        var errors = new List<string>();
        int successCount = 0;
        int failureCount = 0;

        // Get course offering with all details
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null)
        {
            errors.Add("Course offering not found.");
            return (0, 1, errors);
        }

        // Check if offering is in a valid state for enrollment
        if (offering.Status == "Cancelled" || offering.Status == "Completed")
        {
            errors.Add($"Cannot enroll students: Offering status is '{offering.Status}'.");
            return (0, 1, errors);
        }

        // Check if term is valid for new enrollments
        if (offering.Term.Status == "Completed")
        {
            errors.Add("Cannot enroll students: Term has already been completed.");
            return (0, 1, errors);
        }

        // Determine target students based on offering criteria
        IEnumerable<Student> targetStudents;

        if (offering.SectionId.HasValue)
        {
            // SCENARIO 1: Offering has SectionId -> Enroll ONLY students in that section
            targetStudents = await _unitOfWork.Students.GetBySectionAsync(offering.SectionId.Value);
        }
        else
        {
            // SCENARIO 2: Offering has NO SectionId -> Enroll students by Department
            var course = offering.Course;
            targetStudents = await _unitOfWork.Students.GetByDepartmentAsync(course.DepartmentId);
        }

        // Filter: Only active students with active academic status
        targetStudents = targetStudents
            .Where(s => s.IsActive && s.AcademicStatus == "Active")
            .ToList();

        // Validate and enroll each student
        foreach (var student in targetStudents)
        {
            // Use existing validation
            var (isEligible, validationMessage) = await ValidateEnrollmentAsync(student.Id, courseOfferingId);

            if (!isEligible)
            {
                failureCount++;
                errors.Add($"{student.FullName} ({student.StudentCode}): {validationMessage}");
                continue;
            }

            // Enroll student
            var enrollment = new StudentEnrollment
            {
                StudentId = student.Id,
                CourseOfferingId = courseOfferingId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Enrolled"
            };

            await _unitOfWork.StudentEnrollments.AddAsync(enrollment);
            successCount++;
        }

        // Save all enrollments in one transaction
        if (successCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return (successCount, failureCount, errors);
    }

    public async Task<(int SuccessCount, int FailureCount, List<string> Errors)> EnrollStudentsByIdsAsync(
        int courseOfferingId,
        List<int> studentIds)
    {
        var errors = new List<string>();
        int successCount = 0;
        int failureCount = 0;

        // Validate offering exists
        var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(courseOfferingId);
        if (offering == null)
        {
            errors.Add("Course offering not found.");
            return (0, studentIds.Count, errors);
        }

        foreach (var studentId in studentIds)
        {
            // Get student
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null)
            {
                failureCount++;
                errors.Add($"Student ID {studentId}: Not found.");
                continue;
            }

            // Validate enrollment
            var (isEligible, message) = await ValidateEnrollmentAsync(studentId, courseOfferingId);

            if (!isEligible)
            {
                failureCount++;
                errors.Add($"{student.FullName} ({student.StudentCode}): {message}");
                continue;
            }

            // Enroll student
            var enrollment = new StudentEnrollment
            {
                StudentId = studentId,
                CourseOfferingId = courseOfferingId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Enrolled"
            };

            await _unitOfWork.StudentEnrollments.AddAsync(enrollment);
            successCount++;
        }

        // Save all enrollments
        if (successCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return (successCount, failureCount, errors);
    }

    public async Task<(bool Success, string Message)> UnenrollStudentAsync(int enrollmentId)
    {
        var enrollment = await _unitOfWork.StudentEnrollments.GetByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            return (false, "Enrollment not found.");
        }

        if (enrollment.Status != "Enrolled")
        {
            return (false, $"Cannot unenroll: Enrollment status is '{enrollment.Status}'.");
        }

        // Change status to Dropped instead of deleting
        enrollment.Status = "Dropped";
        _unitOfWork.StudentEnrollments.Update(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Student unenrolled successfully.");
    }

    public async Task<IEnumerable<StudentEligibilityDto>> GetEligibleStudentsAsync(
        int courseOfferingId,
        string? searchTerm = null)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null)
        {
            return Enumerable.Empty<StudentEligibilityDto>();
        }

        // Determine target students (same logic as AutoEnroll)
        IEnumerable<Student> targetStudents;

        if (offering.SectionId.HasValue)
        {
            targetStudents = await _unitOfWork.Students.GetBySectionAsync(offering.SectionId.Value);
        }
        else
        {
            var course = offering.Course;
            targetStudents = await _unitOfWork.Students.GetByDepartmentAsync(course.DepartmentId);
        }

        // Filter active students
        targetStudents = targetStudents
            .Where(s => s.IsActive && s.AcademicStatus == "Active")
            .ToList();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            targetStudents = targetStudents.Where(s =>
                s.FullName.ToLower().Contains(searchTerm) ||
                (s.StudentCode != null && s.StudentCode.ToLower().Contains(searchTerm))
            );
        }

        // Get already enrolled students
        var enrolledStudentIds = (await _unitOfWork.StudentEnrollments.GetOfferingEnrollmentsAsync(courseOfferingId))
            .Where(e => e.Status == "Enrolled")
            .Select(e => e.StudentId)
            .ToHashSet();

        // Build eligibility DTOs
        var result = new List<StudentEligibilityDto>();
        foreach (var student in targetStudents)
        {
            bool isAlreadyEnrolled = enrolledStudentIds.Contains(student.Id);

            result.Add(new StudentEligibilityDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                StudentCode = student.StudentCode,
                Email = student.Email,
                DepartmentName = student.Department?.Name,
                LevelName = student.Level?.Name,
                SectionName = student.Section?.Name,
                IsAlreadyEnrolled = isAlreadyEnrolled,
                IsEligible = !isAlreadyEnrolled
            });
        }

        return result.OrderBy(s => s.StudentName).ToList();
    }
}
