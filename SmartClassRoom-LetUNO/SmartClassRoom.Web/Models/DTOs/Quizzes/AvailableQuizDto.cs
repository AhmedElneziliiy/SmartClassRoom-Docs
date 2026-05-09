namespace SmartClassRoom.Web.Models.DTOs.Quizzes;

public class AvailableQuizDto
{
    public int AssignmentId { get; set; }
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimit { get; set; }
    public decimal PassingScore { get; set; }
    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableUntil { get; set; }
    public int QuestionCount { get; set; }
    public int AttemptsUsed { get; set; }
    public int? MaxAttempts { get; set; }
    public bool CanAttempt { get; set; }
}
