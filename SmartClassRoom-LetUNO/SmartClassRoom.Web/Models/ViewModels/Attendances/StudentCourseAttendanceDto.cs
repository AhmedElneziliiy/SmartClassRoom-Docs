namespace SmartClassRoom.Web.Models.ViewModels.Attendances
{
    /// <summary>
    /// DTO for displaying a specific student's attendance in a specific course offering
    /// </summary>
    public class StudentCourseAttendanceDto
    {
        // Student Information
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string? StudentEmail { get; set; }
        public string? StudentPhone { get; set; }

        // Course Offering Information
        public int CourseOfferingId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;

        // Overall Statistics
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
        public decimal AttendancePercentage { get; set; }
        public int LateArrivals { get; set; }
        public int AverageLateMinutes { get; set; }

        // Session-by-Session Details
        public List<StudentSessionAttendanceDto> Sessions { get; set; } = new List<StudentSessionAttendanceDto>();

        // Status Distribution
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();

        // Monthly Breakdown
        public List<MonthlyAttendanceDto> MonthlyBreakdown { get; set; } = new List<MonthlyAttendanceDto>();
    }

    public class StudentSessionAttendanceDto
    {
        public int SessionId { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Topic { get; set; }
        public string Status { get; set; } = string.Empty; // Present, Absent, Late, Excused
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? MinutesLate { get; set; }
        public string? VerificationMethod { get; set; }
        public string? Notes { get; set; }
    }

    public class MonthlyAttendanceDto
    {
        public string Month { get; set; } = string.Empty;
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public decimal AttendanceRate { get; set; }
    }
}
