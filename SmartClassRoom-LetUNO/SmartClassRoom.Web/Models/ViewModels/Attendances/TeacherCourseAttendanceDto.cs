namespace SmartClassRoom.Web.Models.ViewModels.Attendances
{
    /// <summary>
    /// DTO for displaying a specific teacher's attendance in a specific course offering
    /// </summary>
    public class TeacherCourseAttendanceDto
    {
        // Teacher Information
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherCode { get; set; } = string.Empty;
        public string? TeacherEmail { get; set; }
        public string? TeacherPhone { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        // Course Offering Information
        public int CourseOfferingId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public int TotalEnrolledStudents { get; set; }

        // Overall Statistics
        public int TotalScheduledSessions { get; set; }
        public int SessionsAttended { get; set; }
        public int SessionsMissed { get; set; }
        public int LateArrivals { get; set; }
        public decimal AttendancePercentage { get; set; }
        public int AverageLateMinutes { get; set; }
        public int TotalTeachingHours { get; set; }

        // Session-by-Session Details
        public List<TeacherSessionAttendanceDto> Sessions { get; set; } = new List<TeacherSessionAttendanceDto>();

        // Status Distribution
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();

        // Monthly Breakdown
        public List<MonthlyTeachingAttendanceDto> MonthlyBreakdown { get; set; } = new List<MonthlyTeachingAttendanceDto>();
    }

    public class TeacherSessionAttendanceDto
    {
        public int SessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan ScheduledStartTime { get; set; }
        public TimeSpan ScheduledEndTime { get; set; }
        public string? Topic { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Present, Absent, Late
        public DateTime? ActualCheckInTime { get; set; }
        public DateTime? ActualCheckOutTime { get; set; }
        public int? MinutesLate { get; set; }
        public string? VerificationMethod { get; set; }
        public int StudentsPresent { get; set; }
        public int TotalStudents { get; set; }
        public decimal StudentAttendanceRate { get; set; }
        public string? Notes { get; set; }
    }

    public class MonthlyTeachingAttendanceDto
    {
        public string Month { get; set; } = string.Empty;
        public int ScheduledSessions { get; set; }
        public int AttendedSessions { get; set; }
        public int MissedSessions { get; set; }
        public int LateArrivals { get; set; }
        public decimal AttendanceRate { get; set; }
    }
}
