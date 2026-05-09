using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Timetables;

public class GenerateTimetableRequest : IValidatableObject
{
    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Level is required")]
    public int LevelId { get; set; }

    public int? SectionId { get; set; }

    [Required(ErrorMessage = "Term is required")]
    public int TermId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Working days configuration
    [Required(ErrorMessage = "At least one working day must be selected")]
    public List<string> SelectedDays { get; set; } = new List<string>();

    // Time range configuration
    [Required(ErrorMessage = "Start time is required")]
    public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);  // Default 8:00 AM

    [Required(ErrorMessage = "End time is required")]
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);   // Default 5:00 PM

    // Slot duration configuration - Fixed at 60 minutes
    public int SlotDurationMinutes => 60;  // Fixed at 60 minutes per slot

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "End time must be after start time",
                new[] { nameof(EndTime) });
        }

        if (SelectedDays == null || !SelectedDays.Any())
        {
            yield return new ValidationResult(
                "At least one working day must be selected",
                new[] { nameof(SelectedDays) });
        }
    }
}
