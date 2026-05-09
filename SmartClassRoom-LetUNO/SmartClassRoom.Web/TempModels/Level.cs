using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Level
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int LevelNumber { get; set; }

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
