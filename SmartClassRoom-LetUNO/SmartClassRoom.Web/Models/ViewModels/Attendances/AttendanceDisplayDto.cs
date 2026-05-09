namespace SmartClassRoom.Web.Models.ViewModels.Attendances
{
    /// <summary>
    /// DTO for displaying attendance records in the Attendances management page
    /// Supports both student and teacher attendance
    /// </summary>
    public class AttendanceDisplayDto
    {
        public DateTime CheckInTime { get; set; }

        /// <summary>
        /// Type of attendance: "Student" or "Teacher"
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the student or teacher
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Course name for the session
        /// </summary>
        public string CourseName { get; set; } = string.Empty;

        /// <summary>
        /// Formatted session time (e.g., "09:00 - 10:30")
        /// </summary>
        public string SessionTime { get; set; } = string.Empty;

        /// <summary>
        /// Status: Present, Absent, Late, Excused
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Verification method: Face, Manual, ESP32
        /// </summary>
        public string VerificationMethod { get; set; } = string.Empty;

        /// <summary>
        /// Check out time (if available)
        /// </summary>
        public DateTime? CheckOutTime { get; set; }
    }
}
