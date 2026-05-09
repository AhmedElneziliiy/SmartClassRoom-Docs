using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartClassRoom.Web.Models.Entities.Grading;

public class GradeComponentTemplateItem
{
    public int Id { get; set; }

    public int TemplateId { get; set; }
    public GradeComponentTemplate Template { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string ComponentName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Weight { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal MaxScore { get; set; } = 100;

    public int OrderIndex { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
