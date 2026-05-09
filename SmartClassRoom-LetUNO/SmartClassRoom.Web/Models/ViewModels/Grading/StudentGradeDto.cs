namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class StudentGradeDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseOfferingId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public List<ComponentGradeDto> ComponentGrades { get; set; } = new();
    public decimal? FinalGrade { get; set; }
    public string? LetterGrade { get; set; }
}

public class ComponentGradeDto
{
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal MaxScore { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime? EnteredAt { get; set; }
}
