namespace SmartClassRoom.Web.Models.ViewModels.Attendances
{
    /// <summary>
    /// DTO for session performance analytics
    /// Combines both student and teacher attendance for a session
    /// </summary>
    public class SessionPerformanceDto
    {
        public int SessionId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseCode { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Total expected attendees (enrolled students + teacher)
        /// </summary>
        public int TotalExpected { get; set; }

        /// <summary>
        /// Total present (students + teacher)
        /// </summary>
        public int TotalPresent { get; set; }

        /// <summary>
        /// Attendance rate percentage
        /// </summary>
        public decimal AttendanceRate { get; set; }

        /// <summary>
        /// Number of students present
        /// </summary>
        public int StudentCount { get; set; }

        /// <summary>
        /// Number of teachers present (0 or 1)
        /// </summary>
        public int TeacherCount { get; set; }
    }
}
