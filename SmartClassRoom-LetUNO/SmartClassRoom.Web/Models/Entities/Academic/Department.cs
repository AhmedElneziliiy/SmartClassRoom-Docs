using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Academic department entity
    /// </summary>
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(20)]
        public string? Code { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int UniversityId { get; set; }

        public int? HeadOfDepartmentId { get; set; } // Foreign key to Teacher

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual University University { get; set; }
        public virtual Teacher? HeadOfDepartment { get; set; }

        // Collections
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<Level> Levels { get; set; } = new List<Level>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}
