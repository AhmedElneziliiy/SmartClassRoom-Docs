using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Role
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Name { get; set; }

    public string? NormalizedName { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public virtual ICollection<RoleClaim> RoleClaimApplicationRoles { get; set; } = new List<RoleClaim>();

    public virtual ICollection<RoleClaim> RoleClaimRoles { get; set; } = new List<RoleClaim>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
