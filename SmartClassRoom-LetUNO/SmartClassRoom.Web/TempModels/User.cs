using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? NationalId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? ProfilePhotoPath { get; set; }

    public string? FaceEmbeddingBase64 { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UniversityId { get; set; }

    public string UserType { get; set; } = null!;

    public string? StudentCode { get; set; }

    public int? DepartmentId { get; set; }

    public int? LevelId { get; set; }

    public int? SectionId { get; set; }

    public int? GroupId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public string? AcademicStatus { get; set; }

    public decimal? CurrentGpa { get; set; }

    public string? EmployeeCode { get; set; }

    public int? TeacherDepartmentId { get; set; }

    public string? Title { get; set; }

    public string? Specialization { get; set; }

    public DateTime? HireDate { get; set; }

    public string? EmploymentStatus { get; set; }

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? Email { get; set; }

    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public string? Udid { get; set; }

    public virtual ICollection<Attendance> AttendanceMarkedByNavigations { get; set; } = new List<Attendance>();

    public virtual ICollection<Attendance> AttendanceStudents { get; set; } = new List<Attendance>();

    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Grade> GradeEnteredByNavigations { get; set; } = new List<Grade>();

    public virtual ICollection<Grade> GradeStudents { get; set; } = new List<Grade>();

    public virtual Group? Group { get; set; }

    public virtual Level? Level { get; set; }

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual Section? Section { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<StudentEnrollment> StudentEnrollments { get; set; } = new List<StudentEnrollment>();

    public virtual Department? TeacherDepartment { get; set; }

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();

    public virtual University? University { get; set; }

    public virtual ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

    public virtual ICollection<UserLogin> UserLogins { get; set; } = new List<UserLogin>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
