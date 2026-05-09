using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public int SectionId { get; set; }

    public int? Capacity { get; set; }

    public bool IsActive { get; set; }

    public virtual Section Section { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
