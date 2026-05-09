using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Models.Entities.Grading
{
    /// <summary>
    /// Grade component configuration (Midterm, Final, Assignment, etc.)
    /// </summary>
    public class GradeComponent
    {
        [Key]
        public int Id { get; set; }

        public int CourseOfferingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComponentName { get; set; } // Midterm, Final, Assignment

        [Required]
        public decimal Weight { get; set; } // Percentage (0-100)

        public decimal MaxScore { get; set; } = 100;

        public int OrderIndex { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual CourseOffering CourseOffering { get; set; }

        // Collections
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}
