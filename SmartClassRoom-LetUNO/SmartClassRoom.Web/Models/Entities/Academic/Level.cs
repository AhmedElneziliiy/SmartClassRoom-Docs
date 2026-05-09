using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Academic level/year entity (1st Year, 2nd Year, etc.)
    /// </summary>
    public class Level
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // "Level 1", "Year 1"

        public int LevelNumber { get; set; } // 1, 2, 3, 4

        public int DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Department Department { get; set; }

        // Collections
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
