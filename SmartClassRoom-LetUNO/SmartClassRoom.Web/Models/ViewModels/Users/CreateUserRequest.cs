using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Users;

public class CreateUserRequest
{
    // Basic Information
    [Required(ErrorMessage = "Full Name is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    // User Type and Role
    [Required(ErrorMessage = "User Type is required")]
    public string UserType { get; set; } = "Student"; // "Student" or "Teacher"

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "Student"; // "Admin", "Teacher", "Student"

    // Personal Information
    [MaxLength(20)]
    public string? NationalId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; } // Male/Female

    [MaxLength(500)]
    public string? Address { get; set; }

    // Student-Specific Fields
    [MaxLength(20)]
    public string? StudentCode { get; set; }

    public int? DepartmentId { get; set; }

    public int? LevelId { get; set; }

    public int? SectionId { get; set; }

    public int? GroupId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EnrollmentDate { get; set; }

    [MaxLength(20)]
    public string? AcademicStatus { get; set; } // Active, Suspended, Graduated, etc.

    // Teacher-Specific Fields
    [MaxLength(20)]
    public string? EmployeeCode { get; set; }

    [MaxLength(100)]
    public string? Title { get; set; } // Dr., Prof., etc.

    [MaxLength(100)]
    public string? Specialization { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HireDate { get; set; }

    [MaxLength(20)]
    public string? EmploymentStatus { get; set; } // Full-time, Part-time, etc.

    // Status
    public bool IsActive { get; set; } = true;

    public int? UniversityId { get; set; }
}
