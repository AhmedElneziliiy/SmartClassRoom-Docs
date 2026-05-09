using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Code { get; set; }

    public string? Description { get; set; }

    public int UniversityId { get; set; }

    public int? HeadOfDepartmentId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual User? HeadOfDepartment { get; set; }

    public virtual ICollection<Level> Levels { get; set; } = new List<Level>();

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();

    public virtual University University { get; set; } = null!;

    public virtual ICollection<User> UserDepartments { get; set; } = new List<User>();

    public virtual ICollection<User> UserTeacherDepartments { get; set; } = new List<User>();
}
