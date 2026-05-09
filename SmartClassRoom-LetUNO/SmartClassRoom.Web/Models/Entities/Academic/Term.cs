using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Models.Entities.Academic
{
    /// <summary>
    /// Academic term/semester entity
    /// </summary>
    public class Term
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // "Fall 2025", "Spring 2026"

        [MaxLength(20)]
        public string? Code { get; set; } // "F2025"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Upcoming, Active, Completed

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Collections
        public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
        public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
    }
}
