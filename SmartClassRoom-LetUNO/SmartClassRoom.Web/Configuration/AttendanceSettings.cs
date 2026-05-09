namespace SmartClassRoom.Web.Configuration;

/// <summary>
/// Configuration settings for attendance system
/// </summary>
public class AttendanceSettings
{
    /// <summary>
    /// Grace period in minutes after session start time before marking as "Late"
    /// Default: 15 minutes
    /// </summary>
    public int LateGracePeriodMinutes { get; set; } = 15;

    /// <summary>
    /// Allow check-in before session officially starts (within this many minutes)
    /// Default: 10 minutes early
    /// </summary>
    public int EarlyCheckInMinutes { get; set; } = 10;

    /// <summary>
    /// Require device proximity verification for check-in
    /// </summary>
    public bool RequireDeviceProximity { get; set; } = true;

    /// <summary>
    /// Require device proximity verification for check-out
    /// </summary>
    public bool RequireDeviceProximityForCheckout { get; set; } = false;
}
