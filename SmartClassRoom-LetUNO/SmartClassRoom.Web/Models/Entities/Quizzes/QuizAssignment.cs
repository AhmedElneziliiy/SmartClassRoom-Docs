using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Models.Entities.Quizzes
{
    /// <summary>
    /// Quiz assignment to course offering
    /// </summary>
    public class QuizAssignment
    {
        [Key]
        public int Id { get; set; }

        public int QuizId { get; set; }
        public int CourseOfferingId { get; set; }

        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableUntil { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Quiz Quiz { get; set; }
        public virtual CourseOffering CourseOffering { get; set; }

        // Collections
        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}
