using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Grade
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseOfferingId { get; set; }

    public int GradeComponentId { get; set; }

    public decimal Score { get; set; }

    public string? Feedback { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime EnteredAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual User? EnteredByNavigation { get; set; }

    public virtual GradeComponent GradeComponent { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
