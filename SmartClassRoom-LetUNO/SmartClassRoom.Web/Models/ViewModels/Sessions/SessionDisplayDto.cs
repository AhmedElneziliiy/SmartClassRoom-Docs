namespace SmartClassRoom.Web.Models.ViewModels.Sessions
{
    /// <summary>
    /// DTO for displaying session information in the Sessions management page
    /// </summary>
    public class SessionDisplayDto
    {
        public int SessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public string FormattedDate { get; set; } = string.Empty; // "Jan 26, 2026"
        public string FormattedTime { get; set; } = string.Empty; // "09:00 - 10:30"
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty; // "Room 101 (Building A)"
        public string SessionType { get; set; } = string.Empty; // Regular, Makeup, Exam
        public string Status { get; set; } = string.Empty; // Scheduled, InProgress, Completed, Cancelled
        public string Topic { get; set; } = string.Empty;

        // Attendance metrics
        public int ExpectedStudents { get; set; }
        public int StudentsPresent { get; set; }
        public decimal StudentAttendanceRate { get; set; }
        public bool TeacherPresent { get; set; }

        // Timestamps
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        // Flags
        public bool IsUpcoming { get; set; }
        public bool IsActive { get; set; }
        public bool IsToday { get; set; }
    }
}
