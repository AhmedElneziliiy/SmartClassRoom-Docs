using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Class section entity (Section A, Section B, etc.)
    /// </summary>
    public class Section
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // "Section A"

        [MaxLength(10)]
        public string? Code { get; set; } // "A", "B", "C"

        public int LevelId { get; set; }

        public int? Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Level Level { get; set; }

        // Collections
        public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    }
}
