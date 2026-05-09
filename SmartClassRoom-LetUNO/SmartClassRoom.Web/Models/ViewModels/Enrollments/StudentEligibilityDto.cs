namespace SmartClassRoom.Web.Models.ViewModels.Enrollments;

/// <summary>
/// DTO for displaying student eligibility in enrollment management UI
/// </summary>
public class StudentEligibilityDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? Email { get; set; }
    public string? DepartmentName { get; set; }
    public string? LevelName { get; set; }
    public string? SectionName { get; set; }
    public bool IsAlreadyEnrolled { get; set; }
    public bool IsEligible { get; set; }
}
