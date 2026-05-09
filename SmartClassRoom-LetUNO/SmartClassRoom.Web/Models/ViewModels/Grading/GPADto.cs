namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class GPADto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal CumulativeGPA { get; set; }
    public decimal? TermGPA { get; set; }
    public int TotalCredits { get; set; }
    public int? TermCredits { get; set; }
    public decimal TotalQualityPoints { get; set; }
    public int? TermId { get; set; }
}
