using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Attendance
{
    /// <summary>
    /// Student attendance record
    /// </summary>
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        public int SessionId { get; set; }
        public int StudentId { get; set; }

        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        public DateTime? CheckOutTime { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Present, Absent, Late, Excused

        [MaxLength(50)]
        public string? VerificationMethod { get; set; } // Face, Manual, ESP32

        public int? MarkedBy { get; set; } // Teacher ID if manual

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int? ESPDeviceId { get; set; }

        // Navigation Properties
        public virtual Session Session { get; set; }
        public virtual Student Student { get; set; }
        public virtual Teacher? MarkedByTeacher { get; set; }
        public virtual ESPDevice? ESPDevice { get; set; }
    }
}
