using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Models.ViewModels.Enrollments;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IStudentEnrollmentService _enrollmentService;

    public EnrollmentsController(IStudentEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Enroll a student in a course offering
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnrollStudent([FromBody] EnrollStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message) = await _enrollmentService.EnrollStudentAsync(
            request.StudentId,
            request.CourseOfferingId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Drop a course enrollment
    /// </summary>
    [HttpDelete("{enrollmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DropCourse(int enrollmentId)
    {
        var (success, message) = await _enrollmentService.DropCourseAsync(enrollmentId);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }

    /// <summary>
    /// Get all enrollments for a student
    /// </summary>
    [HttpGet("student/{studentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentEnrollments(int studentId, [FromQuery] int? termId = null)
    {
        var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId, termId);

        var response = enrollments.Select(e => new EnrollmentResponse
        {
            EnrollmentId = e.Id,
            StudentName = e.Student.FullName,
            CourseName = e.CourseOffering.Course.Name,
            TermName = e.CourseOffering.Term.Name,
            EnrollmentDate = e.EnrollmentDate,
            Status = e.Status
        });

        return Ok(response);
    }

    /// <summary>
    /// Get all enrollments for a course offering
    /// </summary>
    [HttpGet("offering/{offeringId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOfferingEnrollments(int offeringId)
    {
        var enrollments = await _enrollmentService.GetOfferingEnrollmentsAsync(offeringId);

        var response = enrollments.Select(e => new EnrollmentResponse
        {
            EnrollmentId = e.Id,
            StudentName = e.Student.FullName,
            CourseName = e.CourseOffering.Course.Name,
            TermName = e.CourseOffering.Term.Name,
            EnrollmentDate = e.EnrollmentDate,
            Status = e.Status
        });

        return Ok(response);
    }

    /// <summary>
    /// Validate if a student is eligible to enroll in a course offering
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateEnrollment([FromBody] EnrollStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (isEligible, message) = await _enrollmentService.ValidateEnrollmentAsync(
            request.StudentId,
            request.CourseOfferingId);

        return Ok(new { isEligible, message });
    }
}
