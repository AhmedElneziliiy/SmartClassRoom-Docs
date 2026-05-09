using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.ViewModels.Attendances;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AttendancesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendancesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: Admin/Attendances
    public async Task<IActionResult> Index(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? verificationMethod = null,
        string? status = null,
        int? courseId = null,
        string? attendanceType = null)
    {
        // Get date range (default: last 30 days)
        var endDateValue = endDate ?? DateTime.UtcNow;
        var startDateValue = startDate ?? endDateValue.AddDays(-30);

        // Get all student attendance records (navigation properties should already be loaded)
        var studentAttendancesQuery = await _unitOfWork.Attendances.GetAllAsync();
        var studentAttendances = studentAttendancesQuery
            .Where(a => a.Session != null && a.Session.SessionDate >= startDateValue && a.Session.SessionDate <= endDateValue)
            .ToList();

        // Get all teacher attendance records (navigation properties should already be loaded)
        var teacherAttendancesQuery = await _unitOfWork.TeacherAttendances.GetAllAsync();
        var teacherAttendances = teacherAttendancesQuery
            .Where(ta => ta.Session != null && ta.Session.SessionDate >= startDateValue && ta.Session.SessionDate <= endDateValue)
            .ToList();

        // Apply filters
        if (!string.IsNullOrEmpty(status))
        {
            studentAttendances = studentAttendances.Where(a => a.Status == status).ToList();
            teacherAttendances = teacherAttendances.Where(ta => ta.Status == status).ToList();
        }

        if (!string.IsNullOrEmpty(verificationMethod))
        {
            studentAttendances = studentAttendances.Where(a => a.VerificationMethod == verificationMethod).ToList();
            teacherAttendances = teacherAttendances.Where(ta => ta.VerificationMethod == verificationMethod).ToList();
        }

        if (courseId.HasValue)
        {
            studentAttendances = studentAttendances
                .Where(a => a.Session?.CourseOffering?.CourseId == courseId.Value).ToList();
            teacherAttendances = teacherAttendances
                .Where(ta => ta.Session?.CourseOffering?.CourseId == courseId.Value).ToList();
        }

        if (!string.IsNullOrEmpty(attendanceType))
        {
            if (attendanceType == "Student")
                teacherAttendances.Clear();
            else if (attendanceType == "Teacher")
                studentAttendances.Clear();
        }

        // Calculate combined totals
        ViewBag.TotalRecords = studentAttendances.Count + teacherAttendances.Count;
        ViewBag.StudentRecords = studentAttendances.Count;
        ViewBag.TeacherRecords = teacherAttendances.Count;

        // Calculate status breakdown (combined)
        ViewBag.PresentCount = studentAttendances.Count(a => a.Status == "Present") +
                              teacherAttendances.Count(ta => ta.Status == "Present");
        ViewBag.AbsentCount = studentAttendances.Count(a => a.Status == "Absent") +
                             teacherAttendances.Count(ta => ta.Status == "Absent");
        ViewBag.LateCount = studentAttendances.Count(a => a.Status == "Late") +
                           teacherAttendances.Count(ta => ta.Status == "Late");
        ViewBag.ExcusedCount = studentAttendances.Count(a => a.Status == "Excused") +
                              teacherAttendances.Count(ta => ta.Status == "Excused");

        ViewBag.PresentPercentage = CalculatePercentage(ViewBag.PresentCount, ViewBag.TotalRecords);
        ViewBag.AbsentPercentage = CalculatePercentage(ViewBag.AbsentCount, ViewBag.TotalRecords);
        ViewBag.LatePercentage = CalculatePercentage(ViewBag.LateCount, ViewBag.TotalRecords);
        ViewBag.ExcusedPercentage = CalculatePercentage(ViewBag.ExcusedCount, ViewBag.TotalRecords);

        // Calculate verification method breakdown (combined)
        ViewBag.FaceVerificationCount = studentAttendances.Count(a => a.VerificationMethod == "Face") +
                                        teacherAttendances.Count(ta => ta.VerificationMethod == "Face");
        ViewBag.ManualVerificationCount = studentAttendances.Count(a => a.VerificationMethod == "Manual") +
                                          teacherAttendances.Count(ta => ta.VerificationMethod == "Manual");
        ViewBag.ESP32VerificationCount = studentAttendances.Count(a => a.VerificationMethod == "ESP32") +
                                         teacherAttendances.Count(ta => ta.VerificationMethod == "ESP32");

        ViewBag.FaceVerificationPercentage = CalculatePercentage(ViewBag.FaceVerificationCount, ViewBag.TotalRecords);
        ViewBag.ManualVerificationPercentage = CalculatePercentage(ViewBag.ManualVerificationCount, ViewBag.TotalRecords);
        ViewBag.ESP32VerificationPercentage = CalculatePercentage(ViewBag.ESP32VerificationCount, ViewBag.TotalRecords);

        // Calculate attendance rates
        var totalStudentSessions = studentAttendances.Select(a => a.SessionId).Distinct().Count();
        var totalTeacherSessions = teacherAttendances.Select(ta => ta.SessionId).Distinct().Count();

        ViewBag.StudentAttendanceRate = totalStudentSessions > 0
            ? Math.Round((decimal)studentAttendances.Count(a => a.Status == "Present") / totalStudentSessions * 100, 1)
            : 0;
        ViewBag.TeacherAttendanceRate = totalTeacherSessions > 0
            ? Math.Round((decimal)teacherAttendances.Count(ta => ta.Status == "Present") / totalTeacherSessions * 100, 1)
            : 0;
        ViewBag.AverageAttendanceRate = (ViewBag.StudentAttendanceRate + ViewBag.TeacherAttendanceRate) / 2;

        // Generate trend data (last 30 days)
        var trendData = Enumerable.Range(0, 30).Select(offset =>
        {
            var date = endDateValue.Date.AddDays(-offset);
            return new
            {
                Date = date.ToString("MMM dd"),
                StudentPresent = studentAttendances.Count(a => a.CheckInTime.Date == date && a.Status == "Present"),
                TeacherPresent = teacherAttendances.Count(ta => ta.CheckInTime.Date == date && ta.Status == "Present"),
                StudentAbsent = studentAttendances.Count(a => a.CheckInTime.Date == date && a.Status == "Absent"),
                TeacherAbsent = teacherAttendances.Count(ta => ta.CheckInTime.Date == date && ta.Status == "Absent")
            };
        }).Reverse().ToList();
        ViewBag.TrendData = trendData;

        // Build recent records list (combine both types, sort by time)
        var recentRecords = studentAttendances
            .OrderByDescending(a => a.CheckInTime)
            .Take(25)
            .Select(a => new AttendanceDisplayDto
            {
                CheckInTime = a.CheckInTime,
                Type = "Student",
                Name = a.Student?.FullName ?? "Unknown",
                CourseName = a.Session?.CourseOffering?.Course?.Name ?? "Unknown",
                SessionTime = a.Session != null
                    ? $"{a.Session.StartTime:hh\\:mm} - {a.Session.EndTime:hh\\:mm}"
                    : "N/A",
                Status = a.Status ?? "Unknown",
                VerificationMethod = a.VerificationMethod ?? "Unknown",
                CheckOutTime = a.CheckOutTime
            })
            .Concat(
                teacherAttendances
                    .OrderByDescending(ta => ta.CheckInTime)
                    .Take(25)
                    .Select(ta => new AttendanceDisplayDto
                    {
                        CheckInTime = ta.CheckInTime,
                        Type = "Teacher",
                        Name = ta.Teacher?.FullName ?? "Unknown",
                        CourseName = ta.Session?.CourseOffering?.Course?.Name ?? "Unknown",
                        SessionTime = ta.Session != null
                            ? $"{ta.Session.StartTime:hh\\:mm} - {ta.Session.EndTime:hh\\:mm}"
                            : "N/A",
                        Status = ta.Status ?? "Unknown",
                        VerificationMethod = ta.VerificationMethod ?? "Unknown",
                        CheckOutTime = ta.CheckOutTime
                    })
            )
            .OrderByDescending(r => r.CheckInTime)
            .Take(50)
            .ToList();
        ViewBag.RecentRecords = recentRecords;

        // Get top/at-risk sessions (simplified for now - can be enhanced later)
        ViewBag.TopSessions = new List<SessionPerformanceDto>();
        ViewBag.AtRiskSessions = new List<SessionPerformanceDto>();

        // Get all courses for filter dropdown
        var courses = await _unitOfWork.Courses.GetAllAsync();
        ViewBag.Courses = new SelectList(courses, "Id", "Name");

        // Filter options
        ViewBag.VerificationMethods = new List<string> { "Face", "Manual", "ESP32" };
        ViewBag.StatusOptions = new List<string> { "Present", "Absent", "Late", "Excused" };
        ViewBag.AttendanceTypes = new List<string> { "Student", "Teacher" };

        // Store filter values
        ViewBag.StartDate = startDateValue.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDateValue.ToString("yyyy-MM-dd");

        return View();
    }

    // GET: Admin/Attendances/StudentCourseAttendance
    public async Task<IActionResult> StudentCourseAttendance(int studentId, int courseOfferingId)
    {
        // Get student
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null)
            return NotFound("Student not found");

        // Get course offering
        var courseOffering = (await _unitOfWork.CourseOfferings.GetAllAsync())
            .FirstOrDefault(co => co.Id == courseOfferingId);
        if (courseOffering == null)
            return NotFound("Course offering not found");

        // Get all sessions for this course offering
        var allSessions = (await _unitOfWork.Sessions.GetAllAsync())
            .Where(s => s.CourseOfferingId == courseOfferingId)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToList();

        // Get all attendance records for this student in this course
        var attendanceRecords = (await _unitOfWork.Attendances.GetAllAsync())
            .Where(a => a.StudentId == studentId && allSessions.Select(s => s.Id).Contains(a.SessionId))
            .ToList();

        // Build session attendance list
        var sessionAttendances = allSessions.Select(session =>
        {
            var attendance = attendanceRecords.FirstOrDefault(a => a.SessionId == session.Id);
            int? minutesLate = null;

            if (attendance != null && attendance.Status == "Late")
            {
                var sessionStart = session.SessionDate.Date.Add(session.StartTime);
                minutesLate = (int)(attendance.CheckInTime - sessionStart).TotalMinutes;
            }

            return new StudentSessionAttendanceDto
            {
                SessionId = session.Id,
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Topic = session.Topic,
                Status = attendance?.Status ?? "Absent",
                CheckInTime = attendance?.CheckInTime,
                CheckOutTime = attendance?.CheckOutTime,
                MinutesLate = minutesLate,
                VerificationMethod = attendance?.VerificationMethod,
                Notes = attendance?.Notes
            };
        }).ToList();

        // Calculate statistics
        var presentCount = sessionAttendances.Count(s => s.Status == "Present");
        var absentCount = sessionAttendances.Count(s => s.Status == "Absent");
        var lateCount = sessionAttendances.Count(s => s.Status == "Late");
        var excusedCount = sessionAttendances.Count(s => s.Status == "Excused");
        var lateArrivals = sessionAttendances.Count(s => s.MinutesLate.HasValue);
        var avgLateMinutes = lateArrivals > 0
            ? (int)sessionAttendances.Where(s => s.MinutesLate.HasValue).Average(s => s.MinutesLate!.Value)
            : 0;

        // Monthly breakdown
        var monthlyBreakdown = sessionAttendances
            .GroupBy(s => s.SessionDate.ToString("yyyy-MM"))
            .Select(g => new MonthlyAttendanceDto
            {
                Month = DateTime.ParseExact(g.Key, "yyyy-MM", null).ToString("MMM yyyy"),
                PresentCount = g.Count(s => s.Status == "Present"),
                AbsentCount = g.Count(s => s.Status == "Absent"),
                LateCount = g.Count(s => s.Status == "Late"),
                AttendanceRate = g.Count() > 0
                    ? Math.Round((decimal)g.Count(s => s.Status == "Present" || s.Status == "Late") / g.Count() * 100, 1)
                    : 0
            })
            .OrderBy(m => m.Month)
            .ToList();

        var dto = new StudentCourseAttendanceDto
        {
            StudentId = student.Id,
            StudentName = student.FullName ?? "Unknown",
            StudentCode = student.StudentCode ?? "",
            StudentEmail = student.Email,
            StudentPhone = student.PhoneNumber,
            CourseOfferingId = courseOffering.Id,
            CourseName = courseOffering.Course?.Name ?? "Unknown",
            CourseCode = courseOffering.Course?.Code ?? "",
            TermName = courseOffering.Term?.Name ?? "Unknown",
            TeacherName = courseOffering.Teacher?.FullName ?? "Unknown",
            TotalSessions = allSessions.Count,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            LateCount = lateCount,
            ExcusedCount = excusedCount,
            AttendancePercentage = allSessions.Count > 0
                ? Math.Round((decimal)(presentCount + lateCount) / allSessions.Count * 100, 1)
                : 0,
            LateArrivals = lateArrivals,
            AverageLateMinutes = avgLateMinutes,
            Sessions = sessionAttendances,
            StatusDistribution = new Dictionary<string, int>
            {
                { "Present", presentCount },
                { "Absent", absentCount },
                { "Late", lateCount },
                { "Excused", excusedCount }
            },
            MonthlyBreakdown = monthlyBreakdown
        };

        return View(dto);
    }

    // GET: Admin/Attendances/TeacherCourseAttendance
    public async Task<IActionResult> TeacherCourseAttendance(int teacherId, int courseOfferingId)
    {
        // Get teacher
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);
        if (teacher == null)
            return NotFound("Teacher not found");

        // Get course offering
        var courseOffering = (await _unitOfWork.CourseOfferings.GetAllAsync())
            .FirstOrDefault(co => co.Id == courseOfferingId);
        if (courseOffering == null)
            return NotFound("Course offering not found");

        // Get all sessions for this course offering
        var allSessions = (await _unitOfWork.Sessions.GetAllAsync())
            .Where(s => s.CourseOfferingId == courseOfferingId)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToList();

        // Get all teacher attendance records for this course
        var teacherAttendanceRecords = (await _unitOfWork.TeacherAttendances.GetAllAsync())
            .Where(ta => ta.TeacherId == teacherId && allSessions.Select(s => s.Id).Contains(ta.SessionId))
            .ToList();

        // Get student attendance for each session to show student attendance rate
        var allStudentAttendances = (await _unitOfWork.Attendances.GetAllAsync())
            .Where(a => allSessions.Select(s => s.Id).Contains(a.SessionId))
            .ToList();

        // Get enrollments to know total students
        var enrollments = (await _unitOfWork.StudentEnrollments.GetAllAsync())
            .Where(e => e.CourseOfferingId == courseOfferingId)
            .ToList();

        // Build session attendance list
        var sessionAttendances = allSessions.Select(session =>
        {
            var attendance = teacherAttendanceRecords.FirstOrDefault(ta => ta.SessionId == session.Id);
            int? minutesLate = null;

            if (attendance != null && attendance.Status == "Late")
            {
                var sessionStart = session.SessionDate.Date.Add(session.StartTime);
                minutesLate = (int)(attendance.CheckInTime - sessionStart).TotalMinutes;
            }

            var sessionStudentAttendances = allStudentAttendances.Where(a => a.SessionId == session.Id).ToList();
            var studentsPresent = sessionStudentAttendances.Count(a => a.Status == "Present" || a.Status == "Late");

            return new TeacherSessionAttendanceDto
            {
                SessionId = session.Id,
                SessionDate = session.SessionDate,
                ScheduledStartTime = session.StartTime,
                ScheduledEndTime = session.EndTime,
                Topic = session.Topic,
                RoomName = session.Room?.Name ?? "TBA",
                Status = attendance?.Status ?? "Absent",
                ActualCheckInTime = attendance?.CheckInTime,
                ActualCheckOutTime = attendance?.CheckOutTime,
                MinutesLate = minutesLate,
                VerificationMethod = attendance?.VerificationMethod,
                StudentsPresent = studentsPresent,
                TotalStudents = enrollments.Count,
                StudentAttendanceRate = enrollments.Count > 0
                    ? Math.Round((decimal)studentsPresent / enrollments.Count * 100, 1)
                    : 0,
                Notes = attendance?.Notes
            };
        }).ToList();

        // Calculate statistics
        var sessionsAttended = sessionAttendances.Count(s => s.Status == "Present" || s.Status == "Late");
        var sessionsMissed = sessionAttendances.Count(s => s.Status == "Absent");
        var lateArrivals = sessionAttendances.Count(s => s.Status == "Late");
        var avgLateMinutes = lateArrivals > 0
            ? (int)sessionAttendances.Where(s => s.MinutesLate.HasValue).Average(s => s.MinutesLate!.Value)
            : 0;

        var totalTeachingHours = allSessions.Sum(s => (int)(s.EndTime - s.StartTime).TotalHours);

        // Monthly breakdown
        var monthlyBreakdown = sessionAttendances
            .GroupBy(s => s.SessionDate.ToString("yyyy-MM"))
            .Select(g => new MonthlyTeachingAttendanceDto
            {
                Month = DateTime.ParseExact(g.Key, "yyyy-MM", null).ToString("MMM yyyy"),
                ScheduledSessions = g.Count(),
                AttendedSessions = g.Count(s => s.Status == "Present" || s.Status == "Late"),
                MissedSessions = g.Count(s => s.Status == "Absent"),
                LateArrivals = g.Count(s => s.Status == "Late"),
                AttendanceRate = g.Count() > 0
                    ? Math.Round((decimal)g.Count(s => s.Status == "Present" || s.Status == "Late") / g.Count() * 100, 1)
                    : 0
            })
            .OrderBy(m => m.Month)
            .ToList();

        var dto = new TeacherCourseAttendanceDto
        {
            TeacherId = teacher.Id,
            TeacherName = teacher.FullName ?? "Unknown",
            TeacherCode = teacher.EmployeeCode ?? "",
            TeacherEmail = teacher.Email,
            TeacherPhone = teacher.PhoneNumber,
            DepartmentName = teacher.Department?.Name ?? "Unknown",
            CourseOfferingId = courseOffering.Id,
            CourseName = courseOffering.Course?.Name ?? "Unknown",
            CourseCode = courseOffering.Course?.Code ?? "",
            TermName = courseOffering.Term?.Name ?? "Unknown",
            TotalEnrolledStudents = enrollments.Count,
            TotalScheduledSessions = allSessions.Count,
            SessionsAttended = sessionsAttended,
            SessionsMissed = sessionsMissed,
            LateArrivals = lateArrivals,
            AttendancePercentage = allSessions.Count > 0
                ? Math.Round((decimal)sessionsAttended / allSessions.Count * 100, 1)
                : 0,
            AverageLateMinutes = avgLateMinutes,
            TotalTeachingHours = totalTeachingHours,
            Sessions = sessionAttendances,
            StatusDistribution = new Dictionary<string, int>
            {
                { "Present", sessionAttendances.Count(s => s.Status == "Present") },
                { "Absent", sessionsMissed },
                { "Late", lateArrivals }
            },
            MonthlyBreakdown = monthlyBreakdown
        };

        return View(dto);
    }

    private decimal CalculatePercentage(int count, int total)
    {
        if (total == 0) return 0;
        return Math.Round((decimal)count / total * 100, 1);
    }
}
