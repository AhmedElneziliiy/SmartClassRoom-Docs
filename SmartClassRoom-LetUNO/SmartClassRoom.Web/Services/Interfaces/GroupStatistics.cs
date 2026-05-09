namespace SmartClassRoom.Web.Services.Interfaces;

public class GroupStatistics
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
    public int ActiveStudentsCount { get; set; }
    public int? Capacity { get; set; }

    // Calculated properties
    public int AvailableCapacity => Capacity.HasValue ? Math.Max(0, Capacity.Value - StudentsCount) : 0;
    public bool IsNearCapacity => Capacity.HasValue && StudentsCount >= (Capacity.Value * 0.9);
}
