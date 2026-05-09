using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Attendance;

namespace SmartClassRoom.Web.Models.Entities.Scheduling
{
    /// <summary>
    /// Classroom/Room entity
    /// </summary>
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Number { get; set; } = string.Empty; // "101", "A-205"

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? RoomType { get; set; } // Lecture, Lab, Auditorium

        public int Capacity { get; set; }

        [MaxLength(100)]
        public string? Building { get; set; }

        [MaxLength(10)]
        public string? Floor { get; set; }

        [MaxLength(500)]
        public string? Equipment { get; set; } // Projector, Whiteboard, Computers

        [MaxLength(20)]
        public string? Status { get; set; } // Available, Maintenance, Reserved

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Device modulation number - set by first ESP device added to this room.
        /// All devices in this room must share the same DeviceId.
        /// </summary>
        [MaxLength(50)]
        public string? DeviceId { get; set; }

        // Collections
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();
        public virtual ICollection<ESPDevice> ESPDevices { get; set; } = new List<ESPDevice>();
    }
}
