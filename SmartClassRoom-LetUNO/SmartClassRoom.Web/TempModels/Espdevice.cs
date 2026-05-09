using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Espdevice
{
    public int Id { get; set; }

    public string DeviceId { get; set; } = null!;

    public string? DeviceName { get; set; }

    public int? RoomId { get; set; }

    public string? MacAddress { get; set; }

    public string? Ipaddress { get; set; }

    public string? Status { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateTime RegisteredAt { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Room? Room { get; set; }
}
