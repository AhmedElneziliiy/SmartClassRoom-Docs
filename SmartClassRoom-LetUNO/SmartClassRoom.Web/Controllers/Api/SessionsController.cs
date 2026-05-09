using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SessionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SessionsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get session details by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(int id)
    {
        var session = await _unitOfWork.Sessions.GetSessionWithAttendanceAsync(id);

        if (session == null)
            return NotFound(new { message = "Session not found" });

        var offering = await _unitOfWork.CourseOfferings
            .GetOfferingWithDetailsAsync(session.CourseOfferingId);

        return Ok(new
        {
            id = session.Id,
            courseOfferingId = session.CourseOfferingId,
            courseCode = offering.Course.Code,
            courseName = offering.Course.Name,
            teacherName = offering.Teacher?.FullName,
            sessionDate = session.SessionDate,
            startTime = session.StartTime,
            endTime = session.EndTime,
            sessionType = session.SessionType ?? "Regular",
            status = session.Status ?? "Scheduled",
            startedAt = session.StartedAt,
            endedAt = session.EndedAt,
            room = session.Room != null ? new
            {
                id = session.Room.Id,
                name = session.Room.Name,
                building = session.Room.Building,
                capacity = session.Room.Capacity
            } : null,
            attendanceCount = session.AttendanceRecords?.Count ?? 0
        });
    }

    /// <summary>
    /// Get all sessions for a course offering
    /// </summary>
    [HttpGet("offering/{offeringId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessionsByOffering(int offeringId)
    {
        var sessions = await _unitOfWork.Sessions.GetByCourseOfferingAsync(offeringId);

        return Ok(sessions.Select(s => new
        {
            id = s.Id,
            sessionDate = s.SessionDate,
            startTime = s.StartTime,
            endTime = s.EndTime,
            sessionType = s.SessionType ?? "Regular",
            status = s.Status ?? "Scheduled",
            roomName = s.Room?.Name
        }).OrderBy(s => s.sessionDate).ThenBy(s => s.startTime));
    }

    /// <summary>
    /// Get today's sessions for the authenticated user (student or teacher)
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTodaySessions()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        var userType = User.FindFirstValue("UserType");

        IEnumerable<Session> sessions;

        if (userType == "Teacher")
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(userId.Value);
            if (teacher == null)
                return NotFound(new { message = "Teacher record not found" });

            sessions = await _unitOfWork.Sessions.GetTodaySessionsForTeacherAsync(teacher.Id);
        }
        else if (userType == "Student")
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(userId.Value);
            if (student == null)
                return NotFound(new { message = "Student record not found" });

            // Get student's enrolled offerings and their sessions for today
            var enrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(student.Id);
            var todaySessions = new List<Session>();

            foreach (var enrollment in enrollments.Where(e => e.Status == "Enrolled" || e.Status == "Active"))
            {
                var offeringSessions = await _unitOfWork.Sessions.GetByCourseOfferingAsync(enrollment.CourseOfferingId);
                todaySessions.AddRange(offeringSessions.Where(s => s.SessionDate.Date == DateTime.Today));
            }

            sessions = todaySessions;
        }
        else
        {
            return BadRequest(new { message = "Invalid user type" });
        }

        return Ok(sessions.Select(s => new
        {
            id = s.Id,
            courseOfferingId = s.CourseOfferingId,
            sessionDate = s.SessionDate,
            startTime = s.StartTime,
            endTime = s.EndTime,
            sessionType = s.SessionType ?? "Regular",
            status = s.Status ?? "Scheduled",
            roomName = s.Room?.Name
        }).OrderBy(s => s.startTime));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }
}
