using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class GradeComponentDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Component name is required")]
    [MaxLength(100)]
    public string ComponentName { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 100, ErrorMessage = "Weight must be between 0.01 and 100")]
    public decimal Weight { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Max score must be between 1 and 1000")]
    public decimal MaxScore { get; set; } = 100;

    public int OrderIndex { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
