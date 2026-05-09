using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Enrollments;

/// <summary>
/// Request for bulk enrolling students
/// </summary>
public class BulkEnrollRequest
{
    [Required]
    public int CourseOfferingId { get; set; }

    [Required]
    public List<int> StudentIds { get; set; } = new List<int>();
}
