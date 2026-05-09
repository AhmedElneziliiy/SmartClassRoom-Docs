using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Courses;

/// <summary>
/// ViewModel for editing an existing Course
/// </summary>
public class EditCourseRequest
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Course Name is required")]
    [MaxLength(200, ErrorMessage = "Course Name cannot exceed 200 characters")]
    [Display(Name = "Course Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course Code is required")]
    [MaxLength(20, ErrorMessage = "Course Code cannot exceed 20 characters")]
    [Display(Name = "Course Code")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Credits is required")]
    [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
    [Display(Name = "Credits")]
    public int Credits { get; set; }

    [MaxLength(50, ErrorMessage = "Course Type cannot exceed 50 characters")]
    [Display(Name = "Course Type")]
    public string? CourseType { get; set; }

    [Required(ErrorMessage = "Department is required")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Display(Name = "Prerequisite Course")]
    public int? PrerequisiteCourseId { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;
}
