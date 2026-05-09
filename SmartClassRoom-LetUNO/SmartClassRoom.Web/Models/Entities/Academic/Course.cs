using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Course catalog entity
    /// </summary>
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } // CS101, MATH201

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int Credits { get; set; }

        [MaxLength(50)]
        public string? CourseType { get; set; } // Lecture, Lab, Hybrid

        public int DepartmentId { get; set; }

        public int? PrerequisiteCourseId { get; set; } // Self-referencing FK

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Department Department { get; set; }
        public virtual Course? PrerequisiteCourse { get; set; }

        // Collections
        public virtual ICollection<CourseOffering> Offerings { get; set; } = new List<CourseOffering>();
        public virtual ICollection<Course> PrerequisiteFor { get; set; } = new List<Course>();
    }
}
