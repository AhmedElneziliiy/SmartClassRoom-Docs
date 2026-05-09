namespace SmartClassRoom.Web.Models.Constants;

public static class EnrollmentStatus
{
    public const string Enrolled = "Enrolled";
    public const string Completed = "Completed";
    public const string Dropped = "Dropped";

    /// <summary>
    /// Checks if a student enrollment status is active (Enrolled or Completed).
    /// Active students can view and have their grades modified.
    /// </summary>
    public static bool IsActive(string? status)
    {
        return status == Enrolled || status == Completed;
    }
}
