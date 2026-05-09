using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.Entities.Quizzes;
using SmartClassRoom.Web.Models.Entities.Grading;
using SmartClassRoom.Web.Models.Entities.Materials;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Course offering entity (specific instance of a course in a term)
    /// </summary>
    public class CourseOffering
    {
        [Key]
        public int Id { get; set; }

        public int CourseId { get; set; }
        public int TermId { get; set; }
        public int TeacherId { get; set; }
        public int? SectionId { get; set; }

        public int SessionsPerWeek { get; set; } = 2;

        public int MaxStudents { get; set; } = 50;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Active, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Course Course { get; set; }
        public virtual Term Term { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual Section? Section { get; set; }

        // Collections
        public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public virtual ICollection<GradeComponent> GradeComponents { get; set; } = new List<GradeComponent>();
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
    }
}
