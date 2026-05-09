namespace SmartClassRoom.Web.Models.ViewModels.Enrollments;

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
