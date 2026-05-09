using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Offerings;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class CourseOfferingService : ICourseOfferingService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseOfferingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CourseOffering>> GetOfferingsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? termId = null,
        int? courseId = null,
        int? teacherId = null,
        int? sectionId = null,
        string? status = null,
        string? sortBy = null,
        bool ascending = true)
    {
        var allOfferings = await _unitOfWork.CourseOfferings.GetAllAsync(
            "Course", "Term", "Teacher", "Section", "Enrollments");

        // Apply filtering
        IEnumerable<CourseOffering> filtered = allOfferings;

        if (termId.HasValue)
        {
            filtered = filtered.Where(o => o.TermId == termId.Value);
        }

        if (courseId.HasValue)
        {
            filtered = filtered.Where(o => o.CourseId == courseId.Value);
        }

        if (teacherId.HasValue)
        {
            filtered = filtered.Where(o => o.TeacherId == teacherId.Value);
        }

        if (sectionId.HasValue)
        {
            filtered = filtered.Where(o => o.SectionId == sectionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filtered = filtered.Where(o => o.Status == status);
        }

        // Apply sorting
        IEnumerable<CourseOffering> sorted = (sortBy?.ToLower(), ascending) switch
        {
            ("course", true) => filtered.OrderBy(o => o.Course.Name),
            ("course", false) => filtered.OrderByDescending(o => o.Course.Name),
            ("term", true) => filtered.OrderBy(o => o.Term.StartDate),
            ("term", false) => filtered.OrderByDescending(o => o.Term.StartDate),
            ("teacher", true) => filtered.OrderBy(o => o.Teacher.FullName),
            ("teacher", false) => filtered.OrderByDescending(o => o.Teacher.FullName),
            ("section", true) => filtered.OrderBy(o => o.Section != null ? o.Section.Name : ""),
            ("section", false) => filtered.OrderByDescending(o => o.Section != null ? o.Section.Name : ""),
            ("status", true) => filtered.OrderBy(o => o.Status),
            ("status", false) => filtered.OrderByDescending(o => o.Status),
            _ => filtered.OrderByDescending(o => o.Term.StartDate) // Default: newest term first
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<CourseOffering>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CourseOffering?> GetOfferingByIdAsync(int id)
    {
        return await _unitOfWork.CourseOfferings.GetByIdAsync(id);
    }

    public async Task<CourseOffering?> GetOfferingWithDetailsAsync(int id)
    {
        return await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(id);
    }

    public async Task<OfferingStatistics> GetOfferingStatisticsAsync(int id)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithEnrollmentsAsync(id);

        if (offering == null)
        {
            return new OfferingStatistics();
        }

        var enrolledCount = offering.Enrollments.Count;
        var activeEnrollmentsCount = offering.Enrollments.Count(e => e.Status == "Enrolled");
        var availableSeats = offering.MaxStudents - enrolledCount;
        var isNearCapacity = enrolledCount >= (offering.MaxStudents * 0.9);

        return new OfferingStatistics
        {
            OfferingId = offering.Id,
            CourseName = offering.Course.Name,
            CourseCode = offering.Course.Code,
            TermName = offering.Term.Name,
            TeacherName = offering.Teacher.FullName,
            MaxStudents = offering.MaxStudents,
            EnrolledStudentsCount = enrolledCount,
            ActiveEnrollmentsCount = activeEnrollmentsCount,
            AvailableSeats = availableSeats,
            IsNearCapacity = isNearCapacity
        };
    }

    public async Task<IEnumerable<CourseOffering>> GetOfferingsByTermAsync(int termId)
    {
        return await _unitOfWork.CourseOfferings.GetByTermAsync(termId);
    }

    public async Task<IEnumerable<CourseOffering>> GetOfferingsByTeacherAsync(int teacherId)
    {
        return await _unitOfWork.CourseOfferings.GetByTeacherAsync(teacherId);
    }

    public async Task<(bool Success, string Message, int? OfferingId)> CreateOfferingAsync(CreateOfferingRequest request)
    {
        // Validate course exists and is active
        var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId);
        if (course == null)
        {
            return (false, "Course not found.", null);
        }

        if (!course.IsActive)
        {
            return (false, "Cannot create offering for inactive course.", null);
        }

        // Validate term exists and is active
        var term = await _unitOfWork.Terms.GetByIdAsync(request.TermId);
        if (term == null)
        {
            return (false, "Term not found.", null);
        }

        if (!term.IsActive)
        {
            return (false, "Cannot create offering for inactive term.", null);
        }

        // Validate teacher exists
        var teacher = await _unitOfWork.Repository<Models.Entities.Identity.ApplicationUser>()
            .GetByIdAsync(request.TeacherId);
        if (teacher == null)
        {
            return (false, "Teacher not found.", null);
        }

        // Validate section if provided
        if (request.SectionId.HasValue)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(request.SectionId.Value);
            if (section == null)
            {
                return (false, "Section not found.", null);
            }

            if (!section.IsActive)
            {
                return (false, "Cannot create offering for inactive section.", null);
            }
        }

        // Validate max students
        if (request.MaxStudents <= 0)
        {
            return (false, "Max students must be greater than zero.", null);
        }

        var offering = new CourseOffering
        {
            CourseId = request.CourseId,
            TermId = request.TermId,
            TeacherId = request.TeacherId,
            SectionId = request.SectionId,
            SessionsPerWeek = request.SessionsPerWeek,
            MaxStudents = request.MaxStudents,
            Notes = request.Notes,
            Status = request.Status
        };

        await _unitOfWork.CourseOfferings.AddAsync(offering);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course offering created successfully.", offering.Id);
    }

    public async Task<(bool Success, string Message)> UpdateOfferingAsync(EditOfferingRequest request)
    {
        var offering = await _unitOfWork.CourseOfferings.GetByIdWithDetailsAsync(request.Id);
        if (offering == null)
        {
            return (false, "Course offering not found.");
        }

        // Validate course exists and is active
        var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId);
        if (course == null)
        {
            return (false, "Course not found.");
        }

        if (!course.IsActive)
        {
            return (false, "Cannot update offering with inactive course.");
        }

        // Validate term exists and is active
        var term = await _unitOfWork.Terms.GetByIdAsync(request.TermId);
        if (term == null)
        {
            return (false, "Term not found.");
        }

        if (!term.IsActive)
        {
            return (false, "Cannot update offering with inactive term.");
        }

        // Validate teacher exists
        var teacher = await _unitOfWork.Repository<Models.Entities.Identity.ApplicationUser>()
            .GetByIdAsync(request.TeacherId);
        if (teacher == null)
        {
            return (false, "Teacher not found.");
        }

        // Validate section if provided
        if (request.SectionId.HasValue)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(request.SectionId.Value);
            if (section == null)
            {
                return (false, "Section not found.");
            }

            if (!section.IsActive)
            {
                return (false, "Cannot update offering with inactive section.");
            }
        }

        // Validate max students (cannot reduce below current enrollment count)
        var currentEnrollmentCount = offering.Enrollments.Count(e => e.Status == "Enrolled");
        if (request.MaxStudents < currentEnrollmentCount)
        {
            return (false, $"Cannot reduce max students below current enrollment count ({currentEnrollmentCount}).");
        }

        // Update properties
        offering.CourseId = request.CourseId;
        offering.TermId = request.TermId;
        offering.TeacherId = request.TeacherId;
        offering.SectionId = request.SectionId;
        offering.SessionsPerWeek = request.SessionsPerWeek;
        offering.MaxStudents = request.MaxStudents;
        offering.Notes = request.Notes;
        offering.Status = request.Status;

        _unitOfWork.CourseOfferings.Update(offering);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course offering updated successfully.");
    }

    public async Task<(bool Success, string Message)> CancelOfferingAsync(int id)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithEnrollmentsAsync(id);
        if (offering == null)
        {
            return (false, "Course offering not found.");
        }

        if (offering.Status == "Cancelled")
        {
            return (false, "Course offering is already cancelled.");
        }

        // Check if has active enrollments
        var hasActiveEnrollments = offering.Enrollments.Any(e => e.Status == "Enrolled");
        if (hasActiveEnrollments)
        {
            return (false, "Cannot cancel offering with active enrollments.");
        }

        offering.Status = "Cancelled";
        _unitOfWork.CourseOfferings.Update(offering);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Course offering cancelled successfully.");
    }
}
