using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class RoleClaim
{
    public int Id { get; set; }

    public int? ApplicationRoleId { get; set; }

    public int RoleId { get; set; }

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public virtual Role? ApplicationRole { get; set; }

    public virtual Role Role { get; set; } = null!;
}
