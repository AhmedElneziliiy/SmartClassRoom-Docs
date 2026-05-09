using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Term
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Status { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();

    public virtual ICollection<Timetable> TimetableTermId1Navigations { get; set; } = new List<Timetable>();

    public virtual ICollection<Timetable> TimetableTerms { get; set; } = new List<Timetable>();
}
