using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.DTOs.Attendance;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AttendanceController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get list of courses the student is enrolled in with attendance summary
    /// </summary>
    [HttpGet("my-courses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CourseAttendanceListDto>>> GetMyCourses([FromQuery] int? termId = null)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        var userType = User.FindFirstValue("UserType");
        if (userType != "Student")
            return BadRequest(new { message = "This endpoint is for students only" });

        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(userId.Value);
        if (student == null)
            return NotFound(new { message = "Student record not found" });

        // Get active term if not specified
        if (!termId.HasValue)
        {
            var currentTerm = await _unitOfWork.Terms.GetCurrentTermAsync();
            termId = currentTerm?.Id;
        }

        var enrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(student.Id, termId);
        var coursesList = new List<CourseAttendanceListDto>();

        foreach (var enrollment in enrollments.Where(e => e.Status == "Enrolled" || e.Status == "Active"))
        {
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);
            var stats = await _unitOfWork.Attendances.GetAttendanceStatisticsByCourseAsync(student.Id, offering.Id);

            coursesList.Add(new CourseAttendanceListDto
            {
                CourseOfferingId = offering.Id,
                CourseCode = offering.Course.Code,
                CourseName = offering.Course.Name,
                TermName = offering.Term.Name,
                TeacherName = offering.Teacher?.FullName ?? "TBA",
                TotalSessions = stats.TotalSessions,
                AttendedSessions = stats.PresentCount + stats.LateCount,
                AttendancePercentage = stats.AttendancePercentage
            });
        }

        return Ok(coursesList.OrderBy(c => c.CourseCode));
    }

    /// <summary>
    /// Get detailed attendance report for a student in a specific course
    /// </summary>
    [HttpGet("course/{offeringId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAttendanceReportDto>> GetCourseAttendanceReport(int offeringId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        var userType = User.FindFirstValue("UserType");
        if (userType != "Student")
            return BadRequest(new { message = "This endpoint is for students only" });

        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(userId.Value);
        if (student == null)
            return NotFound(new { message = "Student record not found" });

        // Verify student is enrolled in this offering
        var isEnrolled = await _unitOfWork.StudentEnrollments.IsStudentEnrolledAsync(student.Id, offeringId);
        if (!isEnrolled)
            return StatusCode(403, new { message = "You are not enrolled in this course" });

        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offeringId);
        if (offering == null)
            return NotFound(new { message = "Course offering not found" });

        // Get attendance statistics
        var stats = await _unitOfWork.Attendances.GetAttendanceStatisticsByCourseAsync(student.Id, offeringId);

        // Get detailed attendance records
        var attendances = await _unitOfWork.Attendances.GetByStudentAndCourseOfferingAsync(student.Id, offeringId);

        var records = attendances.Select(a => new AttendanceRecordDto
        {
            SessionId = a.SessionId,
            SessionDate = a.Session.SessionDate,
            StartTime = a.Session.StartTime,
            EndTime = a.Session.EndTime,
            Status = a.Status,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            Notes = a.Notes
        }).OrderByDescending(r => r.SessionDate).ToList();

        return Ok(new UserAttendanceReportDto
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            CourseOfferingId = offeringId,
            CourseCode = offering.Course.Code,
            CourseName = offering.Course.Name,
            TermName = offering.Term.Name,
            TotalSessions = stats.TotalSessions,
            PresentCount = stats.PresentCount,
            AbsentCount = stats.AbsentCount,
            LateCount = stats.LateCount,
            ExcusedCount = stats.ExcusedCount,
            AttendancePercentage = stats.AttendancePercentage,
            Records = records
        });
    }

    /// <summary>
    /// Get overall attendance summary for the authenticated student
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttendanceSummary()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        var userType = User.FindFirstValue("UserType");
        if (userType != "Student")
            return BadRequest(new { message = "This endpoint is for students only" });

        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(userId.Value);
        if (student == null)
            return NotFound(new { message = "Student record not found" });

        // Get overall attendance statistics
        var overallStats = await _unitOfWork.Attendances.GetAttendanceStatisticsAsync(student.Id);

        return Ok(new
        {
            studentId = student.Id,
            studentName = student.FullName,
            totalSessions = overallStats.TotalSessions,
            presentCount = overallStats.PresentCount,
            absentCount = overallStats.AbsentCount,
            lateCount = overallStats.LateCount,
            excusedCount = overallStats.ExcusedCount,
            attendancePercentage = overallStats.AttendancePercentage
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }
}
