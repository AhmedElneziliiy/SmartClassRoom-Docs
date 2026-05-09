using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Student group within a section (for lab sessions, projects)
    /// </summary>
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // "Group 1"

        [MaxLength(10)]
        public string? Code { get; set; } // "G1", "G2"

        public int SectionId { get; set; }

        public int? Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Section Section { get; set; }

        // Collections
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
