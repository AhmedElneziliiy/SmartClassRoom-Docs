using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Models.Entities.Grading;

public class GradeComponentTemplate
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string TemplateName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<GradeComponentTemplateItem> Items { get; set; } = new List<GradeComponentTemplateItem>();
}
