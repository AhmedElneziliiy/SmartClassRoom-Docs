using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class StudentEnrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseOfferingId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public string? Status { get; set; }

    public decimal? FinalGrade { get; set; }

    public string? LetterGrade { get; set; }

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
