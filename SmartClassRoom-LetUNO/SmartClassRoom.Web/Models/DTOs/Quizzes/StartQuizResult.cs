using SmartClassRoom.Web.Models.Enums;

namespace SmartClassRoom.Web.Models.DTOs.Quizzes;

public class StartQuizResult
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimit { get; set; }
    public DateTime StartedAt { get; set; }
    public List<QuizQuestionForStudent> Questions { get; set; } = new();
}

public class QuizQuestionForStudent
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;

    // Question type for frontend rendering
    public QuestionType QuestionType { get; set; }

    public string Option1 { get; set; } = string.Empty;
    public string Option2 { get; set; } = string.Empty;

    // Nullable for True/False questions
    public string? Option3 { get; set; }

    // Nullable for True/False questions
    public string? Option4 { get; set; }

    public decimal Points { get; set; }
    // NO CorrectAnswer or Explanation
}
