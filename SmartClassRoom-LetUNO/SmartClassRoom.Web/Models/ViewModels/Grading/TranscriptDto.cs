namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class TranscriptDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? Email { get; set; }
    public string? DepartmentName { get; set; }
    public string? LevelName { get; set; }
    public List<TranscriptCourseDto> Courses { get; set; } = new();
    public decimal CumulativeGPA { get; set; }
    public int TotalCredits { get; set; }
    public DateTime GeneratedDate { get; set; }
}

public class TranscriptCourseDto
{
    public string Code { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public string TermName { get; set; } = string.Empty;
}
