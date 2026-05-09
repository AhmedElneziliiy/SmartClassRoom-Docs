using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class GradeComponent
{
    public int Id { get; set; }

    public int CourseOfferingId { get; set; }

    public string ComponentName { get; set; } = null!;

    public decimal Weight { get; set; }

    public decimal MaxScore { get; set; }

    public int OrderIndex { get; set; }

    public string? Description { get; set; }

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
