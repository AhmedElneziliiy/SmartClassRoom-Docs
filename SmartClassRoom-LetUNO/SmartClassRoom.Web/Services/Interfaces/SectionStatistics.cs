namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Statistics data for a Section entity
/// </summary>
public class SectionStatistics
{
    public int SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public int GroupsCount { get; set; }
    public int ActiveGroupsCount { get; set; }
    public int StudentsCount { get; set; }
    public int ActiveStudentsCount { get; set; }
    public int CourseOfferingsCount { get; set; }
    public int? Capacity { get; set; }
    public int AvailableCapacity => Capacity.HasValue ? Math.Max(0, Capacity.Value - StudentsCount) : 0;
    public bool IsNearCapacity => Capacity.HasValue && StudentsCount >= (Capacity.Value * 0.9);
}
