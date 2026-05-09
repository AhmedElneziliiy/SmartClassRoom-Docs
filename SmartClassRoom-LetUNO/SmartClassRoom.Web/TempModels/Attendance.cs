using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Attendance
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int StudentId { get; set; }

    public DateTime CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public string? Status { get; set; }

    public string? VerificationMethod { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int? MarkedBy { get; set; }

    public string? Notes { get; set; }

    public int? EspdeviceId { get; set; }

    public virtual Espdevice? Espdevice { get; set; }

    public virtual User? MarkedByNavigation { get; set; }

    public virtual Session Session { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
