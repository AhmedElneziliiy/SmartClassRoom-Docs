using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Models.Entities.Attendance;

/// <summary>
/// Teacher attendance record - tracks teacher check-in/out for sessions
/// </summary>
public class TeacherAttendance
{
    [Key]
    public int Id { get; set; }

    public int SessionId { get; set; }
    public int TeacherId { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    public DateTime? CheckOutTime { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Present, Late, ForceCheckout

    [MaxLength(50)]
    public string? VerificationMethod { get; set; } // ESP32, Manual

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation Properties
    [ForeignKey("SessionId")]
    public virtual Session Session { get; set; } = null!;

    [ForeignKey("TeacherId")]
    public virtual Teacher Teacher { get; set; } = null!;
}
