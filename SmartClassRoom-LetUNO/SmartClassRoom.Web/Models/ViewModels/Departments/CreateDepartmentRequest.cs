using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Departments;

/// <summary>
/// Data Transfer Object for creating a new Department
/// </summary>
public class CreateDepartmentRequest
{
    [Required(ErrorMessage = "Department Name is required")]
    [MaxLength(100, ErrorMessage = "Department Name cannot exceed 100 characters")]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Department Code cannot exceed 20 characters")]
    [Display(Name = "Department Code")]
    public string? Code { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "University is required")]
    [Display(Name = "University")]
    public int UniversityId { get; set; }

    [Display(Name = "Head of Department")]
    public int? HeadOfDepartmentId { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;
}
