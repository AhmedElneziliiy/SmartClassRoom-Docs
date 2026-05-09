using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Quizzes
{
    /// <summary>
    /// Student quiz attempt entity
    /// </summary>
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }

        public int QuizAssignmentId { get; set; }
        public int StudentId { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }

        // Store answers as JSON: { "questionId": selectedOption }
        public string? Answers { get; set; }

        // Store detailed results as JSON
        public string? Results { get; set; }

        public decimal? Score { get; set; }
        public decimal? TotalPoints { get; set; }
        public decimal? Percentage { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // InProgress, Submitted, Graded

        public int AttemptNumber { get; set; } = 1;

        // Navigation Properties
        public virtual QuizAssignment QuizAssignment { get; set; }
        public virtual Student Student { get; set; }
    }
}
