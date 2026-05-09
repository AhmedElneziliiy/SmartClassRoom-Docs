using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Attendance;
using SmartClassRoom.Web.Models.Entities.Quizzes;
using SmartClassRoom.Web.Models.Entities.Grading;
using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.Entities.Users;

/// <summary>
/// Student entity - inherits from ApplicationUser
/// </summary>
public class Student : ApplicationUser
{
    [MaxLength(20)]
    public string? StudentCode { get; set; }

    public int? DepartmentId { get; set; }
    public int? LevelId { get; set; }
    public int? SectionId { get; set; }
    public int? GroupId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    [MaxLength(20)]
    public string? AcademicStatus { get; set; } // Active, Suspended, Graduated

    public decimal? CurrentGPA { get; set; }

    // Navigation Properties
    public virtual Department? Department { get; set; }
    public virtual Level? Level { get; set; }
    public virtual Section? Section { get; set; }
    public virtual Group? Group { get; set; }

    // Collections
    public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
    public virtual ICollection<Attendance.Attendance> AttendanceRecords { get; set; } = new List<Attendance.Attendance>();
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
