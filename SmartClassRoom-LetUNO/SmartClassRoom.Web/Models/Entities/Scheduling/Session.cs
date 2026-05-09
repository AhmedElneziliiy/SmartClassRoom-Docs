using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Attendance;

namespace SmartClassRoom.Web.Models.Entities.Scheduling
{
    /// <summary>
    /// Actual class session entity
    /// </summary>
    public class Session
    {
        [Key]
        public int Id { get; set; }

        public int CourseOfferingId { get; set; }

        public DateTime SessionDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int? RoomId { get; set; }

        [MaxLength(50)]
        public string? SessionType { get; set; } // Regular, Makeup, Exam

        [MaxLength(500)]
        public string? Topic { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Scheduled, InProgress, Completed, Cancelled

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public int? TimetableId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual CourseOffering CourseOffering { get; set; }
        public virtual Room? Room { get; set; }
        public virtual Timetable? Timetable { get; set; }

        // Collections
        public virtual ICollection<Attendance.Attendance> AttendanceRecords { get; set; } = new List<Attendance.Attendance>();
    }
}
