using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Room
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public string? Name { get; set; }

    public string? RoomType { get; set; }

    public int Capacity { get; set; }

    public string? Building { get; set; }

    public string? Floor { get; set; }

    public string? Equipment { get; set; }

    public string? Status { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Espdevice> Espdevices { get; set; } = new List<Espdevice>();

    public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
