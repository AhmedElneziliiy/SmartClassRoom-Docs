using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.ViewModels.Sessions;

namespace SmartClassRoom.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SessionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SessionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? status = null,
            string? sessionType = null,
            int? courseOfferingId = null,
            int? roomId = null,
            int? teacherId = null)
        {
            // Date range setup (default: last 30 days)
            var endDateValue = endDate ?? DateTime.UtcNow;
            var startDateValue = startDate ?? endDateValue.AddDays(-30);

            // Get all sessions in date range
            var sessionsQuery = await _unitOfWork.Sessions.GetByDateRangeAsync(startDateValue, endDateValue);
            var allSessions = sessionsQuery.ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                allSessions = allSessions.Where(s => s.Status == status).ToList();
            }

            if (!string.IsNullOrEmpty(sessionType))
            {
                allSessions = allSessions.Where(s => s.SessionType == sessionType).ToList();
            }

            if (courseOfferingId.HasValue)
            {
                allSessions = allSessions.Where(s => s.CourseOfferingId == courseOfferingId.Value).ToList();
            }

            if (roomId.HasValue)
            {
                allSessions = allSessions.Where(s => s.RoomId == roomId.Value).ToList();
            }

            if (teacherId.HasValue)
            {
                allSessions = allSessions.Where(s => s.CourseOffering?.TeacherId == teacherId.Value).ToList();
            }

            // Calculate statistics - Card 1: Total Sessions
            ViewBag.TotalSessions = allSessions.Count;
            ViewBag.ScheduledCount = allSessions.Count(s => s.Status == "Scheduled");
            ViewBag.InProgressCount = allSessions.Count(s => s.Status == "InProgress");
            ViewBag.CompletedCount = allSessions.Count(s => s.Status == "Completed");
            ViewBag.CancelledCount = allSessions.Count(s => s.Status == "Cancelled");

            // Calculate statistics - Card 2: Completion Rate
            var totalScheduledOrCompleted = ViewBag.ScheduledCount + ViewBag.CompletedCount;
            ViewBag.CompletionRate = totalScheduledOrCompleted > 0
                ? Math.Round((decimal)ViewBag.CompletedCount / totalScheduledOrCompleted * 100, 1)
                : 0;
            ViewBag.CompletedSessions = ViewBag.CompletedCount;
            ViewBag.ScheduledSessions = ViewBag.ScheduledCount;

            // Calculate statistics - Card 3: Session Types
            ViewBag.RegularSessionsCount = allSessions.Count(s => s.SessionType == "Regular");
            ViewBag.MakeupSessionsCount = allSessions.Count(s => s.SessionType == "Makeup");
            ViewBag.ExamSessionsCount = allSessions.Count(s => s.SessionType == "Exam");
            ViewBag.RegularPercentage = CalculatePercentage(ViewBag.RegularSessionsCount, ViewBag.TotalSessions);
            ViewBag.MakeupPercentage = CalculatePercentage(ViewBag.MakeupSessionsCount, ViewBag.TotalSessions);
            ViewBag.ExamPercentage = CalculatePercentage(ViewBag.ExamSessionsCount, ViewBag.TotalSessions);

            // Get attendance data for statistics
            var studentAttendances = (await _unitOfWork.Attendances.GetAllAsync())
                .Where(a => allSessions.Select(s => s.Id).Contains(a.SessionId))
                .ToList();

            var teacherAttendances = (await _unitOfWork.TeacherAttendances.GetAllAsync())
                .Where(ta => allSessions.Select(s => s.Id).Contains(ta.SessionId))
                .ToList();

            var courseOfferingIds = allSessions.Select(s => s.CourseOfferingId).Distinct();
            var enrollments = (await _unitOfWork.StudentEnrollments.GetAllAsync())
                .Where(e => courseOfferingIds.Contains(e.CourseOfferingId))
                .ToList();

            // Calculate statistics - Card 4: Attendance Metrics
            var sessionsWithStudents = allSessions
                .Where(s => enrollments.Any(e => e.CourseOfferingId == s.CourseOfferingId))
                .ToList();

            if (sessionsWithStudents.Any())
            {
                var attendanceRates = sessionsWithStudents.Select(session =>
                {
                    var expectedStudents = enrollments.Count(e => e.CourseOfferingId == session.CourseOfferingId);
                    var presentStudents = studentAttendances
                        .Count(a => a.SessionId == session.Id && (a.Status == "Present" || a.Status == "Late"));
                    return expectedStudents > 0 ? (decimal)presentStudents / expectedStudents * 100 : 0;
                });
                ViewBag.AverageStudentAttendanceRate = Math.Round(attendanceRates.Average(), 1);
            }
            else
            {
                ViewBag.AverageStudentAttendanceRate = 0;
            }

            var sessionsWithTeacherAttendance = allSessions
                .Count(s => teacherAttendances.Any(ta => ta.SessionId == s.Id && ta.Status == "Present"));
            ViewBag.AverageTeacherAttendanceRate = allSessions.Count > 0
                ? Math.Round((decimal)sessionsWithTeacherAttendance / allSessions.Count * 100, 1)
                : 0;

            ViewBag.OverallAttendanceRate =
                (ViewBag.AverageStudentAttendanceRate + ViewBag.AverageTeacherAttendanceRate) / 2;
            ViewBag.SessionsWithAttendance = sessionsWithStudents.Count;

            // Generate chart data - Status Distribution
            ViewBag.StatusDistribution = new
            {
                Scheduled = ViewBag.ScheduledCount,
                InProgress = ViewBag.InProgressCount,
                Completed = ViewBag.CompletedCount,
                Cancelled = ViewBag.CancelledCount
            };

            // Generate chart data - Daily Trend (Last 30 days)
            var trendData = Enumerable.Range(0, 30).Select(offset =>
            {
                var date = endDateValue.Date.AddDays(-offset);
                var sessionsOnDate = allSessions.Where(s => s.SessionDate.Date == date);
                return new
                {
                    Date = date.ToString("MMM dd"),
                    SessionCount = sessionsOnDate.Count(),
                    CompletedCount = sessionsOnDate.Count(s => s.Status == "Completed"),
                    CancelledCount = sessionsOnDate.Count(s => s.Status == "Cancelled")
                };
            }).Reverse().ToList();
            ViewBag.TrendData = trendData;

            // Build recent sessions list
            var recentSessions = allSessions
                .OrderByDescending(s => s.SessionDate)
                .ThenByDescending(s => s.StartTime)
                .Take(50)
                .Select(s => new SessionDisplayDto
                {
                    SessionId = s.Id,
                    SessionDate = s.SessionDate,
                    FormattedDate = s.SessionDate.ToString("MMM dd, yyyy"),
                    FormattedTime = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}",
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    CourseName = s.CourseOffering?.Course?.Name ?? "Unknown",
                    CourseCode = s.CourseOffering?.Course?.Code ?? "N/A",
                    TeacherName = s.CourseOffering?.Teacher?.FullName ?? "Unknown",
                    RoomName = s.Room != null ? $"{s.Room.Number} ({s.Room.Building})" : "TBA",
                    SessionType = s.SessionType ?? "Regular",
                    Status = s.Status ?? "Scheduled",
                    Topic = s.Topic ?? "",
                    ExpectedStudents = enrollments.Count(e => e.CourseOfferingId == s.CourseOfferingId),
                    StudentsPresent = studentAttendances.Count(a =>
                        a.SessionId == s.Id && (a.Status == "Present" || a.Status == "Late")),
                    StudentAttendanceRate = CalculateSessionAttendanceRate(
                        s.Id, studentAttendances, enrollments, s.CourseOfferingId),
                    TeacherPresent = teacherAttendances.Any(ta =>
                        ta.SessionId == s.Id && ta.Status == "Present"),
                    StartedAt = s.StartedAt,
                    EndedAt = s.EndedAt,
                    IsUpcoming = s.SessionDate >= DateTime.UtcNow.Date && s.Status == "Scheduled",
                    IsActive = s.Status == "InProgress",
                    IsToday = s.SessionDate.Date == DateTime.UtcNow.Date
                })
                .ToList();
            ViewBag.RecentSessions = recentSessions;

            // Get upcoming and active sessions
            var upcomingSessions = recentSessions
                .Where(s => s.IsUpcoming && s.SessionDate <= DateTime.UtcNow.Date.AddDays(7))
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .ToList();
            ViewBag.UpcomingSessions = upcomingSessions;
            ViewBag.ActiveSessionsCount = recentSessions.Count(s => s.IsActive);

            // Detect room conflicts (simplified version)
            var roomConflicts = new List<RoomConflictDto>();
            ViewBag.RoomConflicts = roomConflicts;
            ViewBag.HasConflicts = roomConflicts.Any();

            // Prepare dropdown data for filters
            var courseOfferings = await _unitOfWork.CourseOfferings.GetAllAsync();
            var courseOfferingList = courseOfferings
                .Select(co => new
                {
                    Id = co.Id,
                    DisplayName = $"{co.Course?.Code} - {co.Course?.Name} ({co.Term?.Name})"
                })
                .OrderBy(co => co.DisplayName)
                .ToList();
            ViewBag.CourseOfferings = new SelectList(courseOfferingList, "Id", "DisplayName");

            var rooms = await _unitOfWork.Rooms.GetAllAsync();
            var roomList = rooms
                .Select(r => new
                {
                    Id = r.Id,
                    DisplayName = $"{r.Number} - {r.Name ?? ""} ({r.Building ?? "Main"})"
                })
                .OrderBy(r => r.DisplayName)
                .ToList();
            ViewBag.Rooms = new SelectList(roomList, "Id", "DisplayName");

            var teachers = await _unitOfWork.Teachers.GetAllAsync();
            ViewBag.Teachers = new SelectList(teachers.OrderBy(t => t.FullName), "Id", "FullName");

            ViewBag.StatusOptions = new List<string> { "Scheduled", "InProgress", "Completed", "Cancelled" };
            ViewBag.SessionTypeOptions = new List<string> { "Regular", "Makeup", "Exam" };

            // Store filter values
            ViewBag.StartDate = startDateValue.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDateValue.ToString("yyyy-MM-dd");
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedSessionType = sessionType;
            ViewBag.SelectedCourseOfferingId = courseOfferingId;
            ViewBag.SelectedRoomId = roomId;
            ViewBag.SelectedTeacherId = teacherId;

            // Calculate weekly navigation dates
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek); // Sunday
            var endOfWeek = startOfWeek.AddDays(6); // Saturday

            ViewBag.CurrentWeekStart = startOfWeek.ToString("yyyy-MM-dd");
            ViewBag.CurrentWeekEnd = endOfWeek.ToString("yyyy-MM-dd");
            ViewBag.PreviousWeekStart = startOfWeek.AddDays(-7).ToString("yyyy-MM-dd");
            ViewBag.PreviousWeekEnd = endOfWeek.AddDays(-7).ToString("yyyy-MM-dd");
            ViewBag.NextWeekStart = startOfWeek.AddDays(7).ToString("yyyy-MM-dd");
            ViewBag.NextWeekEnd = endOfWeek.AddDays(7).ToString("yyyy-MM-dd");
            ViewBag.MonthStart = new DateTime(today.Year, today.Month, 1).ToString("yyyy-MM-dd");
            ViewBag.MonthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)).ToString("yyyy-MM-dd");

            // Format display range
            ViewBag.DateRangeDisplay = $"{startDateValue:MMM dd} - {endDateValue:MMM dd, yyyy}";

            return View();
        }

        private decimal CalculatePercentage(int count, int total)
        {
            if (total == 0) return 0;
            return Math.Round((decimal)count / total * 100, 1);
        }

        private decimal CalculateSessionAttendanceRate(
            int sessionId,
            IEnumerable<Models.Entities.Attendance.Attendance> attendances,
            IEnumerable<Models.Entities.Academic.StudentEnrollment> enrollments,
            int courseOfferingId)
        {
            var expectedStudents = enrollments.Count(e => e.CourseOfferingId == courseOfferingId);
            if (expectedStudents == 0) return 0;

            var presentStudents = attendances.Count(a =>
                a.SessionId == sessionId &&
                (a.Status == "Present" || a.Status == "Late"));

            return Math.Round((decimal)presentStudents / expectedStudents * 100, 1);
        }
    }
}
