using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Quizzes;

public class UpdateQuizRequest
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Time limit is required")]
    [Range(1, 300, ErrorMessage = "Time limit must be between 1 and 300 minutes")]
    public int TimeLimit { get; set; }

    [Required(ErrorMessage = "Passing score is required")]
    [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
    public decimal PassingScore { get; set; }

    public int? MaxAttempts { get; set; }

    public bool ShuffleQuestions { get; set; } = false;
    public bool ShowCorrectAnswers { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
