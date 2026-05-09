using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Quizzes
{
    /// <summary>
    /// Quiz entity
    /// </summary>
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int CourseId { get; set; }

        public int TimeLimit { get; set; } // in minutes

        public decimal PassingScore { get; set; } // percentage

        public int? MaxAttempts { get; set; }

        public bool ShuffleQuestions { get; set; } = false;
        public bool ShowCorrectAnswers { get; set; } = true;

        public int CreatorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Course Course { get; set; }
        public virtual ApplicationUser Creator { get; set; }

        // Collections
        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
        public virtual ICollection<QuizAssignment> Assignments { get; set; } = new List<QuizAssignment>();
    }
}
