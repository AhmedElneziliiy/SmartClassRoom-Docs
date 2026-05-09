using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Identity;

namespace SmartClassRoom.Web.Models.Entities.Scheduling
{
    /// <summary>
    /// Generated timetable entity
    /// </summary>
    public class Timetable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public int DepartmentId { get; set; }
        public int LevelId { get; set; }
        public int? SectionId { get; set; }
        public int TermId { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Draft, Published

        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? PublishedBy { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Timetable Configuration
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }

        public string WorkingDaysJson { get; set; } = "[]"; // JSON array of selected working days

        [NotMapped]
        public List<string> WorkingDays
        {
            get => string.IsNullOrEmpty(WorkingDaysJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(WorkingDaysJson) ?? new List<string>();
            set => WorkingDaysJson = JsonSerializer.Serialize(value);
        }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Department Department { get; set; }
        public virtual Level Level { get; set; }
        public virtual Section? Section { get; set; }
        public virtual Term Term { get; set; }
        public virtual ApplicationUser? Creator { get; set; }

        // Collections
        public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();
    }
}
