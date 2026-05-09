using SmartClassRoom.Web.Models.Entities.Quizzes;
using SmartClassRoom.Web.Models.ViewModels.Quizzes;
using SmartClassRoom.Web.Models.DTOs.Quizzes;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface IQuizService
{
    // Quiz Management
    Task<(bool Success, string Message, int? QuizId)> CreateQuizAsync(CreateQuizRequest request, int userId, bool isAdmin = false);
    Task<(bool Success, string Message)> UpdateQuizAsync(UpdateQuizRequest request, int userId, bool isAdmin = false);
    Task<(bool Success, string Message)> DeleteQuizAsync(int quizId, int userId, bool isAdmin = false);
    Task<Quiz?> GetQuizByIdAsync(int quizId);
    Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
    Task<IEnumerable<Quiz>> GetQuizzesByCourseAsync(int courseId);
    Task<IEnumerable<Quiz>> GetQuizzesByTeacherAsync(int teacherId);

    // Quiz Assignment
    Task<(bool Success, string Message)> AssignQuizAsync(AssignQuizRequest request, int teacherId);
    Task<(bool Success, string Message)> UnassignQuizAsync(int assignmentId, int userId, bool isAdmin = false);
    Task<IEnumerable<QuizAssignment>> GetAssignmentsByOfferingAsync(int courseOfferingId);

    // Student Quiz Taking
    Task<(bool Success, string Message, StartQuizResult? Result)> StartQuizAsync(int assignmentId, int studentId);
    Task<(bool Success, string Message, QuizResultDto? Result)> SubmitQuizAsync(SubmitQuizRequest request);
    Task<IEnumerable<QuizAssignment>> GetAvailableQuizzesForStudentAsync(int studentId);

    // Grading
    Task<QuizResultDto> GradeQuizAsync(int attemptId);

    // Reporting
    Task<IEnumerable<QuizAttempt>> GetSubmissionsByAssignmentAsync(int assignmentId);
    Task<QuizAttempt?> GetAttemptDetailsAsync(int attemptId);
}
