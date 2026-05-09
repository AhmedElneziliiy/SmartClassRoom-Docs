using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Users;

public class EditUserRequest
{
    [Required]
    public int UserId { get; set; }

    // Basic Information
    [Required(ErrorMessage = "Full Name is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    // Personal Information
    [MaxLength(20)]
    public string? NationalId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    // User Type (cannot be changed, but included for display)
    public string UserType { get; set; } = string.Empty;

    // Role can be updated
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;

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
    public string? AcademicStatus { get; set; }

    public decimal? CurrentGPA { get; set; }

    // Teacher-Specific Fields
    [MaxLength(20)]
    public string? EmployeeCode { get; set; }

    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(100)]
    public string? Specialization { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HireDate { get; set; }

    [MaxLength(20)]
    public string? EmploymentStatus { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    public int? UniversityId { get; set; }
}
