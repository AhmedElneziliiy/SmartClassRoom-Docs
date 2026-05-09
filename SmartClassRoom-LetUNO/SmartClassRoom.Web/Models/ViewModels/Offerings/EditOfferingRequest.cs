using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Offerings;

public class EditOfferingRequest
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Course is required")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Term is required")]
    public int TermId { get; set; }

    [Required(ErrorMessage = "Teacher is required")]
    public int TeacherId { get; set; }

    public int? SectionId { get; set; }

    [Range(1, 7, ErrorMessage = "Sessions per week must be between 1 and 7")]
    public int SessionsPerWeek { get; set; } = 2;

    [Required(ErrorMessage = "Max students is required")]
    [Range(1, 500, ErrorMessage = "Max students must be between 1 and 500")]
    public int MaxStudents { get; set; } = 30;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Active";
}
