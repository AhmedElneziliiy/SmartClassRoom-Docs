using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class GradesController : ControllerBase
{
    private readonly IGradingService _gradingService;
    private readonly ITranscriptService _transcriptService;
    private readonly IUnitOfWork _unitOfWork;

    public GradesController(IGradingService gradingService, ITranscriptService transcriptService, IUnitOfWork unitOfWork)
    {
        _gradingService = gradingService;
        _transcriptService = transcriptService;
        _unitOfWork = unitOfWork;
    }

    // GET: api/Grades/my-courses
    [HttpGet("my-courses")]
    public async Task<ActionResult> GetMyCourses()
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            // Get all enrollments with final grades
            var enrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(studentId);
            var coursesWithGrades = enrollments
                .Where(e => e.Status == "Enrolled" || e.Status == "Completed")
                .Select(e => new
                {
                    CourseOfferingId = e.CourseOfferingId,
                    Code = e.CourseOffering.Course.Code,
                    CourseName = e.CourseOffering.Course.Name,
                    Credits = e.CourseOffering.Course.Credits,
                    TermName = e.CourseOffering.Term.Name,
                    FinalGrade = e.FinalGrade,
                    LetterGrade = e.LetterGrade,
                    Status = e.Status
                })
                .ToList();

            return Ok(coursesWithGrades);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    // GET: api/Grades/course/{offeringId}
    [HttpGet("course/{offeringId}")]
    public async Task<ActionResult> GetCourseGrades(int offeringId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var grades = await _gradingService.GetStudentGradesAsync(studentId, offeringId);
            if (grades == null)
                return NotFound(new { message = "Course not found or you are not enrolled" });

            return Ok(grades);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    // GET: api/Grades/gpa
    [HttpGet("gpa")]
    public async Task<ActionResult> GetGPA([FromQuery] int? termId = null)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var gpa = await _gradingService.CalculateGPAAsync(studentId, termId);
            if (gpa == null)
                return NotFound(new { message = "Student not found" });

            return Ok(gpa);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    // GET: api/Grades/transcript
    [HttpGet("transcript")]
    public async Task<ActionResult> GetTranscript()
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var pdfBytes = await _transcriptService.GenerateTranscriptPdfAsync(studentId);

            return File(pdfBytes, "application/pdf", $"Transcript_{studentId}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    private int GetCurrentStudentId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out int userId) ? userId : 0;
    }
}
