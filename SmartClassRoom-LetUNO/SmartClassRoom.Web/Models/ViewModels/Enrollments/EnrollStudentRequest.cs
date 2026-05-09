using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Enrollments;

public class EnrollStudentRequest
{
    [Required(ErrorMessage = "Student ID is required")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "Course Offering ID is required")]
    public int CourseOfferingId { get; set; }
}
