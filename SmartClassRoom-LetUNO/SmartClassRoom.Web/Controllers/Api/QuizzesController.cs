using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.DTOs.Quizzes;
using SmartClassRoom.Web.Models.ViewModels.Quizzes;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires JWT authentication
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IUnitOfWork _unitOfWork;

    public QuizzesController(IQuizService quizService, IUnitOfWork unitOfWork)
    {
        _quizService = quizService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get available quizzes for the authenticated student
    /// </summary>
    /// <returns>List of available quiz assignments</returns>
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<AvailableQuizDto>>> GetAvailableQuizzes()
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var assignments = await _quizService.GetAvailableQuizzesForStudentAsync(studentId);

            var result = new List<AvailableQuizDto>();
            foreach (var assignment in assignments)
            {
                var attemptCount = await _unitOfWork.QuizAttempts.GetAttemptCountAsync(studentId, assignment.Id);
                var questionCount = await _unitOfWork.QuizQuestions.GetQuestionCountAsync(assignment.QuizId);

                result.Add(new AvailableQuizDto
                {
                    AssignmentId = assignment.Id,
                    QuizId = assignment.QuizId,
                    Title = assignment.Quiz.Title,
                    Description = assignment.Quiz.Description,
                    TimeLimit = assignment.Quiz.TimeLimit,
                    PassingScore = assignment.Quiz.PassingScore,
                    AvailableFrom = assignment.AvailableFrom,
                    AvailableUntil = assignment.AvailableUntil,
                    QuestionCount = questionCount,
                    AttemptsUsed = attemptCount,
                    MaxAttempts = assignment.Quiz.MaxAttempts,
                    CanAttempt = !assignment.Quiz.MaxAttempts.HasValue || attemptCount < assignment.Quiz.MaxAttempts.Value
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Start a new quiz attempt
    /// </summary>
    /// <param name="assignmentId">Quiz assignment ID</param>
    /// <returns>Quiz details with questions (no correct answers)</returns>
    [HttpPost("start/{assignmentId}")]
    public async Task<ActionResult<StartQuizResult>> StartQuiz(int assignmentId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var (success, message, result) = await _quizService.StartQuizAsync(assignmentId, studentId);

            if (!success)
                return BadRequest(new { message });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Submit quiz answers for grading
    /// </summary>
    /// <param name="request">Quiz submission with answers</param>
    /// <returns>Quiz results with score and status</returns>
    [HttpPost("submit")]
    public async Task<ActionResult<QuizResultDto>> SubmitQuiz([FromBody] SubmitQuizRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, result) = await _quizService.SubmitQuizAsync(request);

            if (!success)
                return BadRequest(new { message });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get quiz result for a specific attempt
    /// </summary>
    /// <param name="attemptId">Quiz attempt ID</param>
    /// <returns>Quiz result with score and status</returns>
    [HttpGet("results/{attemptId}")]
    public async Task<ActionResult<QuizResultDto>> GetQuizResult(int attemptId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var attempt = await _unitOfWork.QuizAttempts.GetAttemptWithDetailsAsync(attemptId);

            if (attempt == null)
                return NotFound(new { message = "Attempt not found" });

            if (attempt.StudentId != studentId)
                return Forbid();

            if (attempt.Status != "Graded")
                return BadRequest(new { message = "Quiz has not been graded yet" });

            var result = new QuizResultDto
            {
                AttemptId = attempt.Id,
                Score = attempt.Score ?? 0,
                TotalPoints = attempt.TotalPoints ?? 0,
                Percentage = attempt.Percentage ?? 0,
                Status = attempt.Percentage >= attempt.QuizAssignment.Quiz.PassingScore ? "Passed" : "Failed",
                SubmittedAt = attempt.SubmittedAt ?? DateTime.UtcNow,
                AttemptNumber = attempt.AttemptNumber
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get student's quiz history
    /// </summary>
    /// <param name="assignmentId">Quiz assignment ID</param>
    /// <returns>List of quiz attempts</returns>
    [HttpGet("history/{assignmentId}")]
    public async Task<ActionResult<IEnumerable<QuizResultDto>>> GetQuizHistory(int assignmentId)
    {
        try
        {
            var studentId = GetCurrentStudentId();
            if (studentId == 0)
                return BadRequest(new { message = "Invalid student ID" });

            var attempts = await _unitOfWork.QuizAttempts.GetAttemptsByStudentAsync(studentId, assignmentId);

            var results = attempts
                .Where(a => a.Status == "Graded")
                .Select(a => new QuizResultDto
                {
                    AttemptId = a.Id,
                    Score = a.Score ?? 0,
                    TotalPoints = a.TotalPoints ?? 0,
                    Percentage = a.Percentage ?? 0,
                    Status = a.Percentage >= a.QuizAssignment.Quiz.PassingScore ? "Passed" : "Failed",
                    SubmittedAt = a.SubmittedAt ?? DateTime.UtcNow,
                    AttemptNumber = a.AttemptNumber
                })
                .OrderByDescending(r => r.SubmittedAt);

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
        }
    }

    #region Helper Methods

    private int GetCurrentStudentId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out int userId))
            return userId;
        return 0;
    }

    #endregion
}
