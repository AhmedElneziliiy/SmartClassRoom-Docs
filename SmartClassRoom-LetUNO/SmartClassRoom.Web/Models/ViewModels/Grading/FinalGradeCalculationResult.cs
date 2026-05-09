namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class FinalGradeCalculationResult
{
    public int TotalStudents { get; set; }
    public int SuccessfulStudents { get; set; }
    public int FailedStudents { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}
