using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class CourseOffering
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int TermId { get; set; }

    public int TeacherId { get; set; }

    public int? SectionId { get; set; }

    public int SessionsPerWeek { get; set; }

    public int MaxStudents { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<GradeComponent> GradeComponents { get; set; } = new List<GradeComponent>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<MaterialFolder> MaterialFolders { get; set; } = new List<MaterialFolder>();

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

    public virtual ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();

    public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();

    public virtual Section? Section { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<StudentEnrollment> StudentEnrollments { get; set; } = new List<StudentEnrollment>();

    public virtual User Teacher { get; set; } = null!;

    public virtual Term Term { get; set; } = null!;
}
