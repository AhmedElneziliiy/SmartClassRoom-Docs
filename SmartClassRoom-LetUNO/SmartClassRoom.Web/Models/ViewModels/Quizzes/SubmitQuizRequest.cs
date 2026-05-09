using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Quizzes;

public class SubmitQuizRequest
{
    [Required(ErrorMessage = "Attempt ID is required")]
    public int AttemptId { get; set; }

    [Required(ErrorMessage = "Answers are required")]
    public Dictionary<int, int> Answers { get; set; } = new();
    // Key: QuestionId, Value: Selected option (1-4)
}
