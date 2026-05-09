using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Enums;

namespace SmartClassRoom.Web.Models.Entities.Quizzes
{
    /// <summary>
    /// Quiz question entity - supports Multiple Choice and True/False questions
    /// </summary>
    public class QuizQuestion
    {
        [Key]
        public int Id { get; set; }

        public int QuizId { get; set; }

        [Required]
        public string QuestionText { get; set; }

        // Question type indicator
        [Required]
        public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;

        // Options (1-2 for T/F, 1-4 for MC)
        [Required]
        public string Option1 { get; set; }

        [Required]
        public string Option2 { get; set; }

        // Nullable for True/False questions
        public string? Option3 { get; set; }

        // Nullable for True/False questions
        public string? Option4 { get; set; }

        // Correct answer: 1-2 for T/F, 1-4 for MC
        public int CorrectAnswer { get; set; }

        public decimal Points { get; set; } = 1.0m;

        public int OrderIndex { get; set; }

        [MaxLength(1000)]
        public string? Explanation { get; set; }

        // Navigation Properties
        public virtual Quiz Quiz { get; set; }
    }
}
