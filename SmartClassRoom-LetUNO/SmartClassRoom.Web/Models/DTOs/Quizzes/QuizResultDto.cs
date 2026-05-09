namespace SmartClassRoom.Web.Models.DTOs.Quizzes;

public class QuizResultDto
{
    public int AttemptId { get; set; }
    public decimal Score { get; set; }
    public decimal TotalPoints { get; set; }
    public decimal Percentage { get; set; }
    public string Status { get; set; } = string.Empty; // Passed/Failed
    public DateTime SubmittedAt { get; set; }
    public int AttemptNumber { get; set; }
    // NO correct answers or individual question results
}
