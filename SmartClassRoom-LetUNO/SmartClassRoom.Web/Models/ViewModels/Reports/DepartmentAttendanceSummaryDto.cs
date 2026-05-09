namespace SmartClassRoom.Web.Models.ViewModels.Reports;

/// <summary>
/// DTO for Department Attendance Summary Report
/// Provides system-wide attendance overview by department
/// </summary>
public class DepartmentAttendanceSummaryDto
{
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TermName { get; set; }
    public List<DepartmentAttendanceItemDto> Departments { get; set; } = new();
    public OverallStatistics OverallStats { get; set; } = new();
}

/// <summary>
/// Attendance statistics for a single department
/// </summary>
public class DepartmentAttendanceItemDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalSessions { get; set; }
    public int TotalAttendanceRecords { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public decimal OverallAttendancePercentage { get; set; }
    public int StudentsAtRisk { get; set; } // Students with attendance below 75%
}

/// <summary>
/// System-wide overall statistics
/// </summary>
public class OverallStatistics
{
    public int TotalDepartments { get; set; }
    public int TotalStudents { get; set; }
    public decimal SystemWideAttendancePercentage { get; set; }
    public int TotalStudentsAtRisk { get; set; }
}
