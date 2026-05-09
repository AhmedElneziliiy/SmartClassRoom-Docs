using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public int Credits { get; set; }

    public string? CourseType { get; set; }

    public int DepartmentId { get; set; }

    public int? PrerequisiteCourseId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Course> InversePrerequisiteCourse { get; set; } = new List<Course>();

    public virtual Course? PrerequisiteCourse { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
