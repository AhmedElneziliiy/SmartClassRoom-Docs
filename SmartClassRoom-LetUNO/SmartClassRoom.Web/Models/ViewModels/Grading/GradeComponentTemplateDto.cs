namespace SmartClassRoom.Web.Models.ViewModels.Grading;

public class GradeComponentTemplateDto
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int ComponentCount { get; set; }
    public List<GradeComponentDto> Components { get; set; } = new();
}
