using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Scheduling;

namespace SmartClassRoom.Web.Models.Entities.Attendance
{
    /// <summary>
    /// ESP32-CAM device entity
    /// </summary>
    public class ESPDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string DeviceId { get; set; } // Unique device identifier

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        public int? RoomId { get; set; }

        [MaxLength(20)]
        public string? MacAddress { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Active, Inactive, Maintenance

        public DateTime? LastSeen { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Room? Room { get; set; }

        // Collections
        public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    }
}
