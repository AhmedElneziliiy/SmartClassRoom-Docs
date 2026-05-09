using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class MaterialFolder
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentFolderId { get; set; }

    public int CourseOfferingId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual ICollection<MaterialFolder> InverseParentFolder { get; set; } = new List<MaterialFolder>();

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

    public virtual MaterialFolder? ParentFolder { get; set; }
}
