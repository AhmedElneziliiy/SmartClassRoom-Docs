namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service for performing daily cleanup operations on unclosed records
/// </summary>
public interface IDailyCleanupService
{
    /// <summary>
    /// Performs end-of-day cleanup:
    /// - Force checkout students who didn't check out
    /// - Force checkout teachers who didn't check out
    /// - Force end sessions that are still in progress
    /// </summary>
    /// <returns>Summary of cleanup operations performed</returns>
    Task<CleanupSummary> PerformDailyCleanupAsync();
}

/// <summary>
/// Summary of cleanup operations performed
/// </summary>
public class CleanupSummary
{
    public int SessionsEnded { get; set; }
    public int StudentCheckoutsForced { get; set; }
    public int TeacherCheckoutsForced { get; set; }
    public DateTime CleanupDateTime { get; set; }
    public List<string> Errors { get; set; } = new();
}
