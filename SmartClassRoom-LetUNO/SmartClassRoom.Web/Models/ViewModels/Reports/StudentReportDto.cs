namespace SmartClassRoom.Web.Models.ViewModels.Reports;

public class StudentReportDto
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Level { get; set; }
    public string? Section { get; set; }
    public string? Department { get; set; }
    public List<StudentCourseInfo> CurrentCourses { get; set; } = new();
    public decimal? CurrentTermGPA { get; set; }
    public decimal? CumulativeGPA { get; set; }
    public int TotalCreditsCompleted { get; set; }
    public string CurrentTerm { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
}

public class StudentCourseInfo
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? FinalGrade { get; set; }
    public string? LetterGrade { get; set; }
    public int Credits { get; set; }
    public decimal? AttendancePercentage { get; set; }
}

public class StudentListReportDto
{
    public string? FilterTerm { get; set; }
    public string? FilterLevel { get; set; }
    public string? FilterDepartment { get; set; }
    public List<StudentSummaryDto> Students { get; set; } = new();
    public int TotalStudents { get; set; }
    public DateTime GeneratedDate { get; set; }
}

public class StudentSummaryDto
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string? Section { get; set; }
    public string? Department { get; set; }
    public int CurrentCourses { get; set; }
    public decimal? CurrentGPA { get; set; }
    public decimal? CumulativeGPA { get; set; }
    public string Status { get; set; } = string.Empty;
}
