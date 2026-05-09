using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.ViewModels.Quizzes;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class QuizzesController : Controller
{
    private readonly IQuizService _quizService;
    private readonly IUnitOfWork _unitOfWork;

    public QuizzesController(IQuizService quizService, IUnitOfWork unitOfWork)
    {
        _quizService = quizService;
        _unitOfWork = unitOfWork;
    }

    // GET: Admin/Quizzes
    public async Task<IActionResult> Index(int? courseId)
    {
        var teacherId = GetCurrentUserId();
        var allQuizzes = await _quizService.GetQuizzesByTeacherAsync(teacherId);
        var quizzes = allQuizzes;

        if (courseId.HasValue)
        {
            quizzes = quizzes.Where(q => q.CourseId == courseId.Value);
        }

        // Calculate statistics
        ViewBag.TotalQuizzes = allQuizzes.Count();
        ViewBag.TotalQuestions = allQuizzes.Sum(q => q.Questions?.Count ?? 0);
        ViewBag.AverageTimeLimit = allQuizzes.Any() ? (int)allQuizzes.Average(q => q.TimeLimit) : 0;
        ViewBag.AveragePassingScore = allQuizzes.Any() ? (int)allQuizzes.Average(q => q.PassingScore) : 0;

        // Get assignments count for quizzes
        var assignments = await _unitOfWork.QuizAssignments.GetAllAsync();
        ViewBag.TotalAssignments = assignments.Count();
        ViewBag.ActiveAssignments = assignments.Count(a => a.AvailableUntil >= DateTime.UtcNow);

        // Course distribution
        var quizzesByCourse = allQuizzes.GroupBy(q => q.Course?.Name ?? "Unknown")
                                       .Select(g => new { Course = g.Key, Count = g.Count() })
                                       .OrderByDescending(x => x.Count)
                                       .ToList();
        ViewBag.QuizzesByCourse = quizzesByCourse;

        // Load courses for filter dropdown
        var courses = await _unitOfWork.Courses.GetAllAsync();
        ViewBag.Courses = new SelectList(courses.Where(c => c.IsActive), "Id", "Name");
        ViewBag.SelectedCourseId = courseId;

        return View(quizzes);
    }

    // GET: Admin/Quizzes/Create
    public async Task<IActionResult> Create()
    {
        await LoadCoursesAsync();
        return View();
    }

    // POST: Admin/Quizzes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuizRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            return View(request);
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var (success, message, quizId) = await _quizService.CreateQuizAsync(request, userId, isAdmin);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", message);
        await LoadCoursesAsync();
        return View(request);
    }

    // GET: Admin/Quizzes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await _quizService.GetQuizWithQuestionsAsync(id);
        if (quiz == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        // Only check ownership if not admin
        if (!isAdmin && quiz.CreatorId != userId)
        {
            TempData["ErrorMessage"] = "You are not authorized to edit this quiz";
            return RedirectToAction(nameof(Index));
        }

        var model = new UpdateQuizRequest
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimit = quiz.TimeLimit,
            PassingScore = quiz.PassingScore,
            MaxAttempts = quiz.MaxAttempts,
            ShuffleQuestions = quiz.ShuffleQuestions,
            ShowCorrectAnswers = quiz.ShowCorrectAnswers,
            IsActive = quiz.IsActive
        };

        // Set ViewBag properties for the view
        ViewBag.CourseName = quiz.Course?.Name ?? "Unknown";
        ViewBag.QuestionCount = quiz.Questions?.Count ?? 0;
        ViewBag.TotalPoints = quiz.Questions?.Sum(q => q.Points) ?? 0;
        ViewBag.CreatedAt = quiz.CreatedAt.ToString("MMM dd, yyyy");
        ViewBag.CreatorName = quiz.Creator?.FullName ?? "Unknown";
        ViewBag.HasActiveAssignments = await _unitOfWork.Quizzes.HasActiveAssignmentsAsync(quiz.Id);

        return View(model);
    }

    // POST: Admin/Quizzes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateQuizRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var (success, message) = await _quizService.UpdateQuizAsync(request, userId, isAdmin);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", message);
        return View(request);
    }

    // GET: Admin/Quizzes/Assign/5
    public async Task<IActionResult> Assign(int id)
    {
        var quiz = await _quizService.GetQuizByIdAsync(id);
        if (quiz == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        // Only check ownership if not admin
        if (!isAdmin && quiz.CreatorId != userId)
        {
            TempData["ErrorMessage"] = "You are not authorized to assign this quiz";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Quiz = quiz;
        await LoadCourseOfferingsAsync(quiz.CourseId);

        var model = new AssignQuizRequest
        {
            QuizId = id,
            AvailableFrom = DateTime.Now,
            AvailableUntil = DateTime.Now.AddDays(7)
        };

        return View(model);
    }

    // POST: Admin/Quizzes/Assign
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignQuizRequest request)
    {
        if (!ModelState.IsValid)
        {
            var quiz = await _quizService.GetQuizByIdAsync(request.QuizId);
            ViewBag.Quiz = quiz;
            await LoadCourseOfferingsAsync(quiz!.CourseId);
            return View(request);
        }

        var teacherId = GetCurrentUserId();
        var (success, message) = await _quizService.AssignQuizAsync(request, teacherId);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", message);
        var quizForError = await _quizService.GetQuizByIdAsync(request.QuizId);
        ViewBag.Quiz = quizForError;
        await LoadCourseOfferingsAsync(quizForError!.CourseId);
        return View(request);
    }

    // GET: Admin/Quizzes/Submissions/5
    public async Task<IActionResult> Submissions(int id)
    {
        var assignment = await _unitOfWork.QuizAssignments.GetAssignmentWithQuizAsync(id);
        if (assignment == null)
            return NotFound();

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        // Only check ownership if not admin
        if (!isAdmin && assignment.Quiz.CreatorId != userId)
        {
            TempData["ErrorMessage"] = "You are not authorized to view these submissions";
            return RedirectToAction(nameof(Index));
        }

        var attempts = await _quizService.GetSubmissionsByAssignmentAsync(id);

        ViewBag.Assignment = assignment;
        return View(attempts);
    }

    // POST: Admin/Quizzes/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");
        var (success, message) = await _quizService.DeleteQuizAsync(id, userId, isAdmin);

        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index));
    }

    #region Helper Methods

    private async Task LoadCoursesAsync()
    {
        var courses = await _unitOfWork.Courses.GetAllAsync();
        ViewBag.Courses = new SelectList(courses.Where(c => c.IsActive), "Id", "Name");
    }

    private async Task LoadCourseOfferingsAsync(int courseId)
    {
        var offerings = await _unitOfWork.CourseOfferings.GetByCourseAsync(courseId);

        // Filter to active and upcoming offerings only (not cancelled or completed)
        var currentDate = DateTime.UtcNow;
        var filteredOfferings = offerings
            .Where(o => o.Status != "Cancelled" && o.Status != "Completed") // Exclude cancelled/completed
            .OrderByDescending(o => o.Term.StartDate) // Most recent first
            .ThenBy(o => o.Section?.Name)
            .Select(o => new
            {
                Id = o.Id,
                DisplayName = $"{o.Term.Name} - {o.Section?.Name ?? "No Section"} ({o.Status})"
            }).ToList();

        ViewBag.CourseOfferings = new SelectList(filteredOfferings, "Id", "DisplayName");
        ViewBag.HasCourseOfferings = filteredOfferings.Any();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims. Please log in again.");
        }
        return userId;
    }

    #endregion
}
