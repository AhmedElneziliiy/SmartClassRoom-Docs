using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class SaveGradeDto
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int GradeComponentId { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Score cannot be negative")]
    public decimal Score { get; set; }

    [MaxLength(1000)]
    public string? Feedback { get; set; }

    [Required]
    public int EnteredBy { get; set; }
}
