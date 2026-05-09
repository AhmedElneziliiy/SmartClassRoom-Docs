using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Timetable
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int LevelId { get; set; }

    public int? SectionId { get; set; }

    public int TermId { get; set; }

    public string? Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? PublishedBy { get; set; }

    public DateTime? PublishedAt { get; set; }

    public int? CreatorId { get; set; }

    public int? TermId1 { get; set; }

    public virtual User? Creator { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Level Level { get; set; } = null!;

    public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();

    public virtual Section? Section { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual Term Term { get; set; } = null!;

    public virtual Term? TermId1Navigation { get; set; }
}
