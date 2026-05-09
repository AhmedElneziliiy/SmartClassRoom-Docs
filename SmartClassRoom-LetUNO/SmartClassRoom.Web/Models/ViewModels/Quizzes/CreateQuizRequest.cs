using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Enums;

namespace SmartClassRoom.Web.Models.ViewModels.Quizzes;

public class CreateQuizRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Course is required")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Time limit is required")]
    [Range(1, 300, ErrorMessage = "Time limit must be between 1 and 300 minutes")]
    public int TimeLimit { get; set; } // minutes

    [Required(ErrorMessage = "Passing score is required")]
    [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
    public decimal PassingScore { get; set; } // percentage

    public int? MaxAttempts { get; set; }

    public bool ShuffleQuestions { get; set; } = false;
    public bool ShowCorrectAnswers { get; set; } = true;

    [Required(ErrorMessage = "At least one question is required")]
    [MinLength(1, ErrorMessage = "At least one question is required")]
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

public class QuizQuestionDto : IValidatableObject
{
    [Required(ErrorMessage = "Question text is required")]
    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Question type is required")]
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;

    [Required(ErrorMessage = "Option 1 is required")]
    public string Option1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option 2 is required")]
    public string Option2 { get; set; } = string.Empty;

    // Optional for True/False questions
    public string? Option3 { get; set; }

    // Optional for True/False questions
    public string? Option4 { get; set; }

    [Required(ErrorMessage = "Correct answer is required")]
    public int CorrectAnswer { get; set; }

    [Range(0.1, 100, ErrorMessage = "Points must be between 0.1 and 100")]
    public decimal Points { get; set; } = 1.0m;

    [MaxLength(1000)]
    public string? Explanation { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate correct answer range based on question type
        if (QuestionType == QuestionType.TrueOrFalse)
        {
            if (CorrectAnswer < 1 || CorrectAnswer > 2)
            {
                yield return new ValidationResult(
                    "Correct answer must be 1 (True) or 2 (False) for True/False questions",
                    new[] { nameof(CorrectAnswer) });
            }

            // Option3 and Option4 should be null/empty for T/F
            if (!string.IsNullOrWhiteSpace(Option3) || !string.IsNullOrWhiteSpace(Option4))
            {
                yield return new ValidationResult(
                    "True/False questions should only have 2 options",
                    new[] { nameof(Option3), nameof(Option4) });
            }
        }
        else if (QuestionType == QuestionType.MultipleChoice)
        {
            if (CorrectAnswer < 1 || CorrectAnswer > 4)
            {
                yield return new ValidationResult(
                    "Correct answer must be between 1 and 4 for Multiple Choice questions",
                    new[] { nameof(CorrectAnswer) });
            }

            // All 4 options required for MC
            if (string.IsNullOrWhiteSpace(Option3))
            {
                yield return new ValidationResult(
                    "Option 3 is required for Multiple Choice questions",
                    new[] { nameof(Option3) });
            }

            if (string.IsNullOrWhiteSpace(Option4))
            {
                yield return new ValidationResult(
                    "Option 4 is required for Multiple Choice questions",
                    new[] { nameof(Option4) });
            }
        }
    }
}
