using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Student enrollment in course offering (junction table)
    /// </summary>
    public class StudentEnrollment
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int CourseOfferingId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string? Status { get; set; } // Enrolled, Dropped, Completed

        public decimal? FinalGrade { get; set; }

        [MaxLength(5)]
        public string? LetterGrade { get; set; } // A, B+, B, C+, etc.

        // Navigation Properties
        public virtual Student Student { get; set; }
        public virtual CourseOffering CourseOffering { get; set; }
    }
}
