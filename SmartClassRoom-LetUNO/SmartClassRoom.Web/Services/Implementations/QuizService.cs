using System.Text.Json;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.DTOs.Quizzes;
using SmartClassRoom.Web.Models.Entities.Quizzes;
using SmartClassRoom.Web.Models.ViewModels.Quizzes;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class QuizService : IQuizService
{
    private readonly IUnitOfWork _unitOfWork;

    public QuizService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region Quiz Management

    public async Task<(bool Success, string Message, int? QuizId)> CreateQuizAsync(CreateQuizRequest request, int userId, bool isAdmin = false)
    {
        try
        {
            // Validate course exists
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId);
            if (course == null)
                return (false, "Course not found", null);

            // Verify authorization: Admin can create for any course, Teacher must be assigned
            if (!isAdmin)
            {
                var courseOfferings = await _unitOfWork.CourseOfferings.GetByCourseAsync(request.CourseId);
                var teacherOwnsCourse = courseOfferings.Any(co => co.TeacherId == userId);

                if (!teacherOwnsCourse)
                    return (false,
                        $"You must be assigned to teach '{course.Name}' before creating quizzes for it. " +
                        "Please ask an administrator to assign you to a course offering for this course.",
                        null);
            }

            // Validate at least one question
            if (request.Questions == null || !request.Questions.Any())
                return (false, "At least one question is required", null);

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Create Quiz entity
                var quiz = new Quiz
                {
                    Title = request.Title,
                    Description = request.Description,
                    CourseId = request.CourseId,
                    TimeLimit = request.TimeLimit,
                    PassingScore = request.PassingScore,
                    MaxAttempts = request.MaxAttempts,
                    ShuffleQuestions = request.ShuffleQuestions,
                    ShowCorrectAnswers = request.ShowCorrectAnswers,
                    CreatorId = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.Quizzes.AddAsync(quiz);
                await _unitOfWork.SaveChangesAsync();

                // Create QuizQuestion entities
                int orderIndex = 0;
                foreach (var questionDto in request.Questions)
                {
                    var question = new QuizQuestion
                    {
                        QuizId = quiz.Id,
                        QuestionText = questionDto.QuestionText,
                        QuestionType = questionDto.QuestionType,
                        Option1 = questionDto.Option1,
                        Option2 = questionDto.Option2,
                        Option3 = questionDto.Option3,
                        Option4 = questionDto.Option4,
                        CorrectAnswer = questionDto.CorrectAnswer,
                        Points = questionDto.Points,
                        OrderIndex = orderIndex++,
                        Explanation = questionDto.Explanation
                    };

                    await _unitOfWork.QuizQuestions.AddAsync(question);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return (true, "Quiz created successfully", quiz.Id);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error creating quiz: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateQuizAsync(UpdateQuizRequest request, int userId, bool isAdmin = false)
    {
        try
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(request.Id);
            if (quiz == null)
                return (false, "Quiz not found");

            // Admin bypass - admins can edit any quiz
            if (!isAdmin && quiz.CreatorId != userId)
                return (false, "You are not authorized to update this quiz");

            // Check if quiz has active assignments
            var hasActiveAssignments = await _unitOfWork.Quizzes.HasActiveAssignmentsAsync(quiz.Id);
            if (hasActiveAssignments)
                return (false, "Cannot update quiz with active assignments");

            quiz.Title = request.Title;
            quiz.Description = request.Description;
            quiz.TimeLimit = request.TimeLimit;
            quiz.PassingScore = request.PassingScore;
            quiz.MaxAttempts = request.MaxAttempts;
            quiz.ShuffleQuestions = request.ShuffleQuestions;
            quiz.ShowCorrectAnswers = request.ShowCorrectAnswers;
            quiz.IsActive = request.IsActive;

            _unitOfWork.Quizzes.Update(quiz);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Quiz updated successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating quiz: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteQuizAsync(int quizId, int userId, bool isAdmin = false)
    {
        try
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null)
                return (false, "Quiz not found");

            // Admin bypass - admins can delete any quiz
            if (!isAdmin && quiz.CreatorId != userId)
                return (false, "You are not authorized to delete this quiz");

            // Check if quiz has any assignments
            var hasActiveAssignments = await _unitOfWork.Quizzes.HasActiveAssignmentsAsync(quizId);
            if (hasActiveAssignments)
                return (false, "Cannot delete quiz with active assignments");

            // Soft delete
            quiz.IsActive = false;
            _unitOfWork.Quizzes.Update(quiz);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Quiz deleted successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error deleting quiz: {ex.Message}");
        }
    }

    public async Task<Quiz?> GetQuizByIdAsync(int quizId)
    {
        return await _unitOfWork.Quizzes.GetByIdAsync(quizId);
    }

    public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId)
    {
        return await _unitOfWork.Quizzes.GetQuizWithQuestionsAsync(quizId);
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByCourseAsync(int courseId)
    {
        return await _unitOfWork.Quizzes.GetQuizzesByCourseAsync(courseId);
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesByTeacherAsync(int teacherId)
    {
        return await _unitOfWork.Quizzes.GetQuizzesByTeacherAsync(teacherId);
    }

    #endregion

    #region Quiz Assignment

    public async Task<(bool Success, string Message)> AssignQuizAsync(AssignQuizRequest request, int teacherId)
    {
        try
        {
            // Validate quiz exists and teacher owns it
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(request.QuizId);
            if (quiz == null)
                return (false, "Quiz not found");

            if (quiz.CreatorId != teacherId)
                return (false, "You are not authorized to assign this quiz");

            // Validate course offering exists
            var courseOffering = await _unitOfWork.CourseOfferings.GetByIdAsync(request.CourseOfferingId);
            if (courseOffering == null)
                return (false, "Course offering not found");

            // Validate dates
            if (request.AvailableFrom >= request.AvailableUntil)
                return (false, "Available from date must be before available until date");

            // Create assignment
            var assignment = new QuizAssignment
            {
                QuizId = request.QuizId,
                CourseOfferingId = request.CourseOfferingId,
                AvailableFrom = request.AvailableFrom,
                AvailableUntil = request.AvailableUntil,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.QuizAssignments.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Quiz assigned successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error assigning quiz: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UnassignQuizAsync(int assignmentId, int userId, bool isAdmin = false)
    {
        try
        {
            var assignment = await _unitOfWork.QuizAssignments.GetAssignmentWithQuizAsync(assignmentId);
            if (assignment == null)
                return (false, "Assignment not found");

            // Admin bypass - admins can unassign any quiz
            if (!isAdmin && assignment.Quiz.CreatorId != userId)
                return (false, "You are not authorized to unassign this quiz");

            await _unitOfWork.QuizAssignments.DeleteAsync(assignmentId);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Quiz unassigned successfully");
        }
        catch (Exception ex)
        {
            return (false, $"Error unassigning quiz: {ex.Message}");
        }
    }

    public async Task<IEnumerable<QuizAssignment>> GetAssignmentsByOfferingAsync(int courseOfferingId)
    {
        return await _unitOfWork.QuizAssignments.GetAssignmentsByCourseOfferingAsync(courseOfferingId);
    }

    #endregion

    #region Student Quiz Taking

    public async Task<(bool Success, string Message, StartQuizResult? Result)> StartQuizAsync(int assignmentId, int studentId)
    {
        try
        {
            var currentTime = DateTime.UtcNow;

            // Validate assignment exists and is active
            var assignment = await _unitOfWork.QuizAssignments.GetAssignmentWithQuizAsync(assignmentId);
            if (assignment == null)
                return (false, "Quiz assignment not found", null);

            if (!assignment.IsPublished)
                return (false, "Quiz is not published", null);

            if (currentTime < assignment.AvailableFrom)
                return (false, "Quiz is not yet available", null);

            if (currentTime > assignment.AvailableUntil)
                return (false, "Quiz is no longer available", null);

            // Check if student is enrolled in the course offering
            var enrollment = await _unitOfWork.StudentEnrollments.GetEnrollmentAsync(studentId, assignment.CourseOfferingId);
            if (enrollment == null)
                return (false, "You are not enrolled in this course", null);

            // Get attempt count for student
            var attemptCount = await _unitOfWork.QuizAttempts.GetAttemptCountAsync(studentId, assignmentId);

            // Validate against MaxAttempts
            if (assignment.Quiz.MaxAttempts.HasValue && attemptCount >= assignment.Quiz.MaxAttempts.Value)
                return (false, $"Maximum attempts ({assignment.Quiz.MaxAttempts.Value}) reached", null);

            // Check for existing in-progress attempt
            var inProgressAttempt = await _unitOfWork.QuizAttempts.GetInProgressAttemptAsync(studentId, assignmentId);
            if (inProgressAttempt != null)
                return (false, "You already have an in-progress attempt for this quiz", null);

            // Create new QuizAttempt
            var attempt = new QuizAttempt
            {
                QuizAssignmentId = assignmentId,
                StudentId = studentId,
                StartedAt = currentTime,
                Status = "InProgress",
                AttemptNumber = attemptCount + 1
            };

            await _unitOfWork.QuizAttempts.AddAsync(attempt);
            await _unitOfWork.SaveChangesAsync();

            // Return quiz questions WITHOUT correct answers
            var result = new StartQuizResult
            {
                AttemptId = attempt.Id,
                QuizId = assignment.Quiz.Id,
                Title = assignment.Quiz.Title,
                Description = assignment.Quiz.Description,
                TimeLimit = assignment.Quiz.TimeLimit,
                StartedAt = attempt.StartedAt,
                Questions = assignment.Quiz.Questions.Select(q => new QuizQuestionForStudent
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Option1 = q.Option1,
                    Option2 = q.Option2,
                    Option3 = q.Option3,
                    Option4 = q.Option4,
                    Points = q.Points
                    // NO CorrectAnswer or Explanation
                }).ToList()
            };

            return (true, "Quiz started successfully", result);
        }
        catch (Exception ex)
        {
            return (false, $"Error starting quiz: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message, QuizResultDto? Result)> SubmitQuizAsync(SubmitQuizRequest request)
    {
        try
        {
            // Get attempt with quiz details
            var attempt = await _unitOfWork.QuizAttempts.GetAttemptWithDetailsAsync(request.AttemptId);
            if (attempt == null)
                return (false, "Attempt not found", null);

            // Validate status is "InProgress"
            if (attempt.Status != "InProgress")
                return (false, "This attempt has already been submitted", null);

            var currentTime = DateTime.UtcNow;

            // Server-side time validation
            var elapsedMinutes = (currentTime - attempt.StartedAt).TotalMinutes;
            var timeLimit = attempt.QuizAssignment.Quiz.TimeLimit;

            if (elapsedMinutes > timeLimit)
                return (false, $"Time limit exceeded. Quiz must be completed within {timeLimit} minutes", null);

            // Save answers as JSON
            attempt.Answers = JsonSerializer.Serialize(request.Answers);
            attempt.SubmittedAt = currentTime;
            attempt.Status = "Submitted";

            _unitOfWork.QuizAttempts.Update(attempt);
            await _unitOfWork.SaveChangesAsync();

            // Grade the quiz
            var result = await GradeQuizAsync(attempt.Id);

            return (true, "Quiz submitted successfully", result);
        }
        catch (Exception ex)
        {
            return (false, $"Error submitting quiz: {ex.Message}", null);
        }
    }

    public async Task<IEnumerable<QuizAssignment>> GetAvailableQuizzesForStudentAsync(int studentId)
    {
        var currentTime = DateTime.UtcNow;
        return await _unitOfWork.QuizAssignments.GetAvailableAssignmentsForStudentAsync(studentId, currentTime);
    }

    #endregion

    #region Grading

    public async Task<QuizResultDto> GradeQuizAsync(int attemptId)
    {
        // Get attempt with quiz and questions
        var attempt = await _unitOfWork.QuizAttempts.GetAttemptWithDetailsAsync(attemptId);
        if (attempt == null)
            throw new InvalidOperationException("Attempt not found");

        var quiz = attempt.QuizAssignment.Quiz;

        // Deserialize answers JSON
        var answers = string.IsNullOrEmpty(attempt.Answers)
            ? new Dictionary<int, int>()
            : JsonSerializer.Deserialize<Dictionary<int, int>>(attempt.Answers) ?? new Dictionary<int, int>();

        // Calculate score
        decimal score = 0;
        decimal totalPoints = 0;
        var questionResults = new List<object>();

        foreach (var question in quiz.Questions)
        {
            totalPoints += question.Points;

            if (answers.TryGetValue(question.Id, out int studentAnswer))
            {
                bool isCorrect = studentAnswer == question.CorrectAnswer;
                if (isCorrect)
                    score += question.Points;

                questionResults.Add(new
                {
                    QuestionId = question.Id,
                    IsCorrect = isCorrect,
                    StudentAnswer = studentAnswer
                });
            }
            else
            {
                // Question not answered
                questionResults.Add(new
                {
                    QuestionId = question.Id,
                    IsCorrect = false,
                    StudentAnswer = (int?)null
                });
            }
        }

        decimal percentage = totalPoints > 0 ? (score / totalPoints) * 100 : 0;
        string status = percentage >= quiz.PassingScore ? "Passed" : "Failed";

        // Save results as JSON
        attempt.Results = JsonSerializer.Serialize(questionResults);
        attempt.Score = score;
        attempt.TotalPoints = totalPoints;
        attempt.Percentage = percentage;
        attempt.Status = "Graded";

        _unitOfWork.QuizAttempts.Update(attempt);
        await _unitOfWork.SaveChangesAsync();

        // Return QuizResultDto (NO correct answers)
        return new QuizResultDto
        {
            AttemptId = attempt.Id,
            Score = score,
            TotalPoints = totalPoints,
            Percentage = percentage,
            Status = status,
            SubmittedAt = attempt.SubmittedAt ?? DateTime.UtcNow,
            AttemptNumber = attempt.AttemptNumber
        };
    }

    #endregion

    #region Reporting

    public async Task<IEnumerable<QuizAttempt>> GetSubmissionsByAssignmentAsync(int assignmentId)
    {
        return await _unitOfWork.QuizAttempts.GetAttemptsByAssignmentAsync(assignmentId);
    }

    public async Task<QuizAttempt?> GetAttemptDetailsAsync(int attemptId)
    {
        return await _unitOfWork.QuizAttempts.GetAttemptWithDetailsAsync(attemptId);
    }

    #endregion
}
