using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Models.Entities.Scheduling
{
    /// <summary>
    /// Scheduled time slot in timetable
    /// </summary>
    public class ScheduledSlot
    {
        [Key]
        public int Id { get; set; }

        public int TimetableId { get; set; }
        public int CourseOfferingId { get; set; }

        [Required]
        [MaxLength(20)]
        public string DayOfWeek { get; set; } // Monday, Tuesday, etc.

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public int? RoomId { get; set; }

        // Navigation Properties
        public virtual Timetable Timetable { get; set; }
        public virtual CourseOffering CourseOffering { get; set; }
        public virtual Room? Room { get; set; }
    }
}
