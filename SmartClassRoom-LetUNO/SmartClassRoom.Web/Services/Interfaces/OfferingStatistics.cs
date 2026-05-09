namespace SmartClassRoom.Web.Services.Interfaces;

public class OfferingStatistics
{
    public int OfferingId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public int EnrolledStudentsCount { get; set; }
    public int ActiveEnrollmentsCount { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsNearCapacity { get; set; }
}
