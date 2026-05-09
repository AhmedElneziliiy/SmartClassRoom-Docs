using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Dashboard service implementation using Unit of Work
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
    {
        // Get all statistics sequentially to avoid DbContext concurrency issues
        var totalStudents = await _unitOfWork.Students.CountAsync();
        var activeStudents = await _unitOfWork.Students.CountAsync(s => s.IsActive);
        var totalTeachers = await _unitOfWork.Teachers.CountAsync();
        var activeTeachers = await _unitOfWork.Teachers.CountAsync(t => t.IsActive);
        var totalDepartments = await _unitOfWork.Departments.CountAsync();
        var activeDepartments = await _unitOfWork.Departments.CountAsync(d => d.IsActive);
        var totalCourses = await _unitOfWork.Courses.CountAsync();
        var activeCourses = await _unitOfWork.Courses.CountAsync(c => c.IsActive);

        var today = DateTime.UtcNow.Date;
        var todaySessions = await _unitOfWork.Sessions.CountAsync(s => s.SessionDate.Date == today);
        var activeSessions = await _unitOfWork.Sessions.CountAsync(s => s.Status == "InProgress");

        return new DashboardStatistics
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            TotalTeachers = totalTeachers,
            ActiveTeachers = activeTeachers,
            TotalDepartments = totalDepartments,
            ActiveDepartments = activeDepartments,
            TotalCourses = totalCourses,
            ActiveCourses = activeCourses,
            TodaySessionsCount = todaySessions,
            ActiveSessionsCount = activeSessions
        };
    }

    public async Task<DepartmentDashboardStatistics> GetDepartmentStatisticsAsync(int departmentId)
    {
        var departmentStats = await _unitOfWork.Departments.GetDepartmentStatisticsAsync(departmentId);

        // Calculate average attendance rate for the department
        var students = await _unitOfWork.Students.GetByDepartmentAsync(departmentId);
        var attendanceRates = new List<decimal>();

        foreach (var student in students)
        {
            var stats = await _unitOfWork.Attendances.GetAttendanceStatisticsAsync(student.Id);
            if (stats.TotalSessions > 0)
            {
                attendanceRates.Add(stats.AttendancePercentage);
            }
        }

        var averageRate = attendanceRates.Any() ? attendanceRates.Average() : 0;

        return new DepartmentDashboardStatistics
        {
            DepartmentId = departmentStats.DepartmentId,
            DepartmentName = departmentStats.DepartmentName,
            StudentCount = departmentStats.StudentCount,
            TeacherCount = departmentStats.TeacherCount,
            CourseCount = departmentStats.CourseCount,
            ActiveCourseOfferingsCount = departmentStats.ActiveCourseOfferingsCount,
            AverageAttendanceRate = Math.Round(averageRate, 2)
        };
    }

    public async Task<IEnumerable<TodaySessionSummary>> GetTodaySessionsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var todaySessions = await _unitOfWork.Sessions.GetByDateAsync(today);

        var summaries = new List<TodaySessionSummary>();

        foreach (var session in todaySessions)
        {
            var attendanceRate = await _unitOfWork.Attendances.GetSessionAttendanceRateAsync(session.Id);

            summaries.Add(new TodaySessionSummary
            {
                SessionId = session.Id,
                CourseName = session.CourseOffering.Course.Name,
                CourseCode = session.CourseOffering.Course.Code,
                TeacherName = session.CourseOffering.Teacher.FullName,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                RoomName = session.Room?.Name,
                Status = session.Status ?? "Scheduled",
                EnrolledStudents = attendanceRate.TotalEnrolled,
                AttendedStudents = attendanceRate.PresentCount
            });
        }

        return summaries.OrderBy(s => s.StartTime);
    }

    public async Task<RecentAttendanceStatistics> GetRecentAttendanceStatisticsAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        var endDate = DateTime.UtcNow;

        var recentAttendance = await _unitOfWork.Attendances.GetByDateRangeAsync(startDate, endDate);
        var attendanceList = recentAttendance.ToList();

        var totalRecords = attendanceList.Count;
        var presentCount = attendanceList.Count(a => a.Status == "Present");
        var absentCount = attendanceList.Count(a => a.Status == "Absent");
        var lateCount = attendanceList.Count(a => a.Status == "Late");

        var averageRate = totalRecords > 0
            ? (decimal)presentCount / totalRecords * 100
            : 0;

        return new RecentAttendanceStatistics
        {
            TotalAttendanceRecords = totalRecords,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            LateCount = lateCount,
            AverageAttendanceRate = Math.Round(averageRate, 2)
        };
    }
}
