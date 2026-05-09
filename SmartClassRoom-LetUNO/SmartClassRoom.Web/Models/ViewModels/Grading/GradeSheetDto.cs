namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class GradeSheetDto
{
    public int CourseOfferingId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public List<GradeComponentDto> Components { get; set; } = new();
    public List<GradeSheetStudentDto> Students { get; set; } = new();
}

public class GradeSheetStudentDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public List<GradeSheetComponentScoreDto> ComponentScores { get; set; } = new();
    public decimal? FinalGrade { get; set; }
    public string? LetterGrade { get; set; }
}

public class GradeSheetComponentScoreDto
{
    public int ComponentId { get; set; }
    public decimal? Score { get; set; }
    public int? GradeId { get; set; }
}
