namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Statistics data for a Level entity
/// </summary>
public class LevelStatistics
{
    public int LevelId { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int LevelNumber { get; set; }
    public int SectionsCount { get; set; }
    public int ActiveSectionsCount { get; set; }
    public int StudentsCount { get; set; }
    public int ActiveStudentsCount { get; set; }
}
