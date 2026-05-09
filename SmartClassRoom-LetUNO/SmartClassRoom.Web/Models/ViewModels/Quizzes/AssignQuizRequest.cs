using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Quizzes;

public class AssignQuizRequest
{
    [Required(ErrorMessage = "Quiz is required")]
    public int QuizId { get; set; }

    [Required(ErrorMessage = "Course offering is required")]
    public int CourseOfferingId { get; set; }

    [Required(ErrorMessage = "Available from date is required")]
    public DateTime AvailableFrom { get; set; }

    [Required(ErrorMessage = "Available until date is required")]
    public DateTime AvailableUntil { get; set; }
}
