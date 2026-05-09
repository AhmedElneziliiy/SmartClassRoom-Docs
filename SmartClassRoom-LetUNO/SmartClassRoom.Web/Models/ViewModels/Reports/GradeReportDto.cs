namespace SmartClassRoom.Web.Models.ViewModels.Reports;

public class GradeReportDto
{
    public int CourseOfferingId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public List<GradeComponentInfo> Components { get; set; } = new();
    public List<GradeReportItemDto> StudentGrades { get; set; } = new();
    public decimal ClassAverage { get; set; }
    public DateTime GeneratedDate { get; set; }
}

public class GradeComponentInfo
{
    public string ComponentName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal MaxScore { get; set; }
}

public class GradeReportItemDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public Dictionary<string, decimal?> ComponentScores { get; set; } = new();
    public decimal? FinalGrade { get; set; }
    public string? LetterGrade { get; set; }
}
