using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Sections;

/// <summary>
/// Request model for editing an existing section
/// </summary>
public class EditSectionRequest
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Section Name is required")]
    [MaxLength(50, ErrorMessage = "Section Name cannot exceed 50 characters")]
    [Display(Name = "Section Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10, ErrorMessage = "Code cannot exceed 10 characters")]
    [Display(Name = "Section Code")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "Level is required")]
    [Display(Name = "Level")]
    public int LevelId { get; set; }

    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
    [Display(Name = "Capacity")]
    public int? Capacity { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; }
}
