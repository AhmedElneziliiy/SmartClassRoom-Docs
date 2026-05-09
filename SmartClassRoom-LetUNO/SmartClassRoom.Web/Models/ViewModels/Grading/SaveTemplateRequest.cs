using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class SaveTemplateRequest
{
    public int? TemplateId { get; set; }

    [Required(ErrorMessage = "Template name is required")]
    [MaxLength(100)]
    public string TemplateName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? DepartmentId { get; set; }

    [Required]
    public int CreatedBy { get; set; }

    [Required]
    public List<GradeComponentDto> Components { get; set; } = new();
}
