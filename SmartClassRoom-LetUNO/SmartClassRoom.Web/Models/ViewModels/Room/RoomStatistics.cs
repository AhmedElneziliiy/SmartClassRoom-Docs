namespace SmartClassRoom.Web.Models.ViewModels.Room;

/// <summary>
/// Statistics for a room including sessions, devices, and scheduled slots
/// </summary>
public class RoomStatistics
{
    public int RoomId { get; set; }

    /// <summary>
    /// Total number of sessions held in this room
    /// </summary>
    public int SessionsCount { get; set; }

    /// <summary>
    /// Number of currently active sessions
    /// </summary>
    public int ActiveSessionsCount { get; set; }

    /// <summary>
    /// Number of ESP devices in this room
    /// </summary>
    public int DevicesCount { get; set; }

    /// <summary>
    /// Number of scheduled slots for this room
    /// </summary>
    public int ScheduledSlotsCount { get; set; }

    /// <summary>
    /// Total attendance records for sessions in this room
    /// </summary>
    public int TotalAttendanceRecords { get; set; }
}
