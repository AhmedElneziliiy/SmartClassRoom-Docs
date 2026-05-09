using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service for performing daily cleanup operations on unclosed records
/// </summary>
public class DailyCleanupService : IDailyCleanupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DailyCleanupService> _logger;

    public DailyCleanupService(
        ApplicationDbContext context,
        ILogger<DailyCleanupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CleanupSummary> PerformDailyCleanupAsync()
    {
        var summary = new CleanupSummary
        {
            CleanupDateTime = DateTime.UtcNow
        };

        _logger.LogInformation("Starting daily cleanup at {DateTime}", summary.CleanupDateTime);

        try
        {
            // 1. Force checkout students who didn't check out
            summary.StudentCheckoutsForced = await ForceCheckoutStudentsAsync();

            // 2. Force checkout teachers who didn't check out
            summary.TeacherCheckoutsForced = await ForceCheckoutTeachersAsync();

            // 3. Force end sessions that are still in progress
            summary.SessionsEnded = await ForceEndSessionsAsync();

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Daily cleanup completed. Sessions ended: {SessionsEnded}, Student checkouts: {StudentCheckouts}, Teacher checkouts: {TeacherCheckouts}",
                summary.SessionsEnded, summary.StudentCheckoutsForced, summary.TeacherCheckoutsForced);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during daily cleanup");
            summary.Errors.Add($"Cleanup failed: {ex.Message}");
        }

        return summary;
    }

    /// <summary>
    /// Force checkout for students who checked in but didn't check out
    /// </summary>
    private async Task<int> ForceCheckoutStudentsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Find attendance records from today or earlier that don't have a checkout time
        var unclosedAttendances = await _context.Attendances
            .Where(a => a.CheckInTime.Date < now.Date && a.CheckOutTime == null)
            .ToListAsync();

        foreach (var attendance in unclosedAttendances)
        {
            // Set checkout time to end of the day they checked in
            var checkInDate = attendance.CheckInTime.Date;
            attendance.CheckOutTime = checkInDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            attendance.Notes = string.IsNullOrEmpty(attendance.Notes)
                ? "Auto checkout - End of day"
                : $"{attendance.Notes} | Auto checkout - End of day";

            _logger.LogInformation(
                "Force checkout for student attendance {AttendanceId}, StudentId: {StudentId}, SessionId: {SessionId}",
                attendance.Id, attendance.StudentId, attendance.SessionId);
        }

        return unclosedAttendances.Count;
    }

    /// <summary>
    /// Force checkout for teachers who checked in but didn't check out
    /// </summary>
    private async Task<int> ForceCheckoutTeachersAsync()
    {
        var now = DateTime.UtcNow;

        // Find teacher attendance records from today or earlier that don't have a checkout time
        var unclosedTeacherAttendances = await _context.TeacherAttendances
            .Where(ta => ta.CheckInTime.Date < now.Date && ta.CheckOutTime == null)
            .ToListAsync();

        foreach (var teacherAttendance in unclosedTeacherAttendances)
        {
            // Set checkout time to end of the day they checked in
            var checkInDate = teacherAttendance.CheckInTime.Date;
            teacherAttendance.CheckOutTime = checkInDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            teacherAttendance.Status = "ForceCheckout";
            teacherAttendance.Notes = string.IsNullOrEmpty(teacherAttendance.Notes)
                ? "Auto checkout - End of day"
                : $"{teacherAttendance.Notes} | Auto checkout - End of day";

            _logger.LogInformation(
                "Force checkout for teacher attendance {AttendanceId}, TeacherId: {TeacherId}, SessionId: {SessionId}",
                teacherAttendance.Id, teacherAttendance.TeacherId, teacherAttendance.SessionId);
        }

        return unclosedTeacherAttendances.Count;
    }

    /// <summary>
    /// Force end sessions that are still in progress
    /// </summary>
    private async Task<int> ForceEndSessionsAsync()
    {
        var now = DateTime.UtcNow;

        // Find sessions from today or earlier that are still in progress
        var unendedSessions = await _context.Sessions
            .Where(s => s.SessionDate.Date < now.Date && s.Status == "InProgress")
            .ToListAsync();

        foreach (var session in unendedSessions)
        {
            session.Status = "Completed";
            // Set ended time to the scheduled end time on the session date
            var sessionDate = session.SessionDate.Date;
            session.EndedAt = sessionDate.Add(session.EndTime);

            _logger.LogInformation(
                "Force end session {SessionId}, CourseOfferingId: {CourseOfferingId}, Date: {SessionDate}",
                session.Id, session.CourseOfferingId, session.SessionDate);
        }

        return unendedSessions.Count;
    }
}
