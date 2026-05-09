using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.Entities.Quizzes;
using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.Entities.Users;

/// <summary>
/// Teacher entity - inherits from ApplicationUser
/// </summary>
public class Teacher : ApplicationUser
{
    [MaxLength(20)]
    public string? EmployeeCode { get; set; }

    public int? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? Title { get; set; } // Prof., Dr., Mr., Ms.

    [MaxLength(100)]
    public string? Specialization { get; set; }

    public DateTime? HireDate { get; set; }

    [MaxLength(20)]
    public string? EmploymentStatus { get; set; } // Full-time, Part-time, Adjunct

    // Navigation Properties
    public virtual Department? Department { get; set; }

    // Collections
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
