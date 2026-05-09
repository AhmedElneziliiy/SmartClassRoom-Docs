using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Section
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public int LevelId { get; set; }

    public int? Capacity { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual Level Level { get; set; } = null!;

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
