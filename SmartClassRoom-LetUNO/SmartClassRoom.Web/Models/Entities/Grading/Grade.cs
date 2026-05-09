using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Grading
{
    /// <summary>
    /// Student grade entity
    /// </summary>
    public class Grade
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int CourseOfferingId { get; set; }
        public int GradeComponentId { get; set; }

        public decimal Score { get; set; }

        [MaxLength(1000)]
        public string? Feedback { get; set; }

        public int? EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Student Student { get; set; }
        public virtual CourseOffering CourseOffering { get; set; }
        public virtual GradeComponent GradeComponent { get; set; }
        public virtual Teacher? EnteredByTeacher { get; set; }
    }
}
