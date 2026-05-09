using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Levels;

/// <summary>
/// Request model for editing an existing level
/// </summary>
public class EditLevelRequest
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Level Name is required")]
    [MaxLength(50, ErrorMessage = "Level Name cannot exceed 50 characters")]
    [Display(Name = "Level Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Level Number is required")]
    [Range(1, 10, ErrorMessage = "Level Number must be between 1 and 10")]
    [Display(Name = "Level Number")]
    public int LevelNumber { get; set; }

    [Required(ErrorMessage = "Department is required")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; }
}
