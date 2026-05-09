using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.DTOs.Timetable;
using SmartClassRoom.Web.Models.ViewModels.Timetables;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TimetablesController : ControllerBase
{
    private readonly ITimetableService _timetableService;
    private readonly IUnitOfWork _unitOfWork;

    public TimetablesController(ITimetableService timetableService, IUnitOfWork unitOfWork)
    {
        _timetableService = timetableService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Generate a new timetable automatically
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TimetableGenerationResult>> Generate(
        [FromBody] GenerateTimetableRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var result = await _timetableService.GenerateTimetableAsync(request, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get timetable details by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetTimetable(int id)
    {
        var timetable = await _timetableService.GetTimetableWithSlotsAsync(id);
        if (timetable == null)
            return NotFound();

        return Ok(timetable);
    }

    /// <summary>
    /// Get all scheduled slots for a timetable
    /// </summary>
    [HttpGet("{id}/slots")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ScheduledSlotDto>>> GetSlots(int id)
    {
        var slots = await _timetableService.GetTimetableSlotsAsync(id);
        return Ok(slots);
    }

    /// <summary>
    /// Detect conflicts in a timetable
    /// </summary>
    [HttpGet("{id}/conflicts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConflictInfo>>> DetectConflicts(int id)
    {
        var conflicts = await _timetableService.DetectConflictsAsync(id);
        return Ok(conflicts);
    }

    /// <summary>
    /// Publish a timetable and generate all sessions
    /// </summary>
    [HttpPost("{id}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PublishTimetableResult>> Publish(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _timetableService.PublishTimetableAsync(id, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update a scheduled slot
    /// </summary>
    [HttpPut("slots/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateSlot(int id, [FromBody] UpdateSlotRequest request)
    {
        if (id != request.SlotId)
            return BadRequest("Slot ID mismatch");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message) = await _timetableService.UpdateSlotAsync(request);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    /// <summary>
    /// Delete a scheduled slot
    /// </summary>
    [HttpDelete("slots/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteSlot(int id)
    {
        var (success, message) = await _timetableService.DeleteSlotAsync(id);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    /// <summary>
    /// Add a new slot to a timetable
    /// </summary>
    [HttpPost("{timetableId}/slots")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddSlot(
        int timetableId,
        [FromQuery] int courseOfferingId,
        [FromBody] TimeSlotDto timeSlot,
        [FromQuery] int? roomId = null)
    {
        var (success, message) = await _timetableService.AddSlotAsync(
            timetableId,
            courseOfferingId,
            timeSlot,
            roomId);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    /// <summary>
    /// Delete a timetable
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteTimetable(int id)
    {
        var (success, message) = await _timetableService.DeleteTimetableAsync(id);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    /// <summary>
    /// Get today's schedule for the authenticated user (Student or Teacher)
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodayScheduleDto>> GetTodaySchedule()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        var userType = User.FindFirstValue("UserType");

        if (userType == "Student")
        {
            return await GetStudentTodaySchedule(userId.Value);
        }
        else if (userType == "Teacher")
        {
            return await GetTeacherTodaySchedule(userId.Value);
        }
        else
        {
            return BadRequest(new { message = "This endpoint is for students and teachers only" });
        }
    }

    /// <summary>
    /// Get full weekly schedule for the authenticated user (Student or Teacher)
    /// </summary>
    [HttpGet("week")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserScheduleDto>> GetWeekSchedule([FromQuery] int? termId = null)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        var userType = User.FindFirstValue("UserType");

        if (userType == "Student")
        {
            return await GetStudentWeekSchedule(userId.Value, termId);
        }
        else if (userType == "Teacher")
        {
            return await GetTeacherWeekSchedule(userId.Value, termId);
        }
        else
        {
            return BadRequest(new { message = "This endpoint is for students and teachers only" });
        }
    }

    /// <summary>
    /// Get schedule for a specific date for the authenticated user (Student or Teacher)
    /// </summary>
    [HttpGet("date/{date}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodayScheduleDto>> GetScheduleByDate(DateTime date)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        var userType = User.FindFirstValue("UserType");

        if (userType == "Student")
        {
            return await GetStudentScheduleByDate(userId.Value, date);
        }
        else if (userType == "Teacher")
        {
            return await GetTeacherScheduleByDate(userId.Value, date);
        }
        else
        {
            return BadRequest(new { message = "This endpoint is for students and teachers only" });
        }
    }

    #region Student Schedule Methods

    private async Task<ActionResult<TodayScheduleDto>> GetStudentTodaySchedule(int studentId)
    {
        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
        if (student == null)
            return NotFound(new { message = "Student record not found" });

        var today = DateTime.Today;
        var todayDayOfWeek = today.DayOfWeek.ToString();

        // Get current term
        var currentTerm = await _unitOfWork.Terms.GetCurrentTermAsync();
        if (currentTerm == null)
            return Ok(new TodayScheduleDto
            {
                UserId = student.Id,
                UserName = student.FullName,
                UserType = "Student",
                StudentId = student.Id,
                StudentName = student.FullName,
                Date = today,
                DayName = todayDayOfWeek,
                Classes = new List<TodayScheduleItemDto>()
            });

        // Get student enrollments for current term
        var enrollments = await _unitOfWork.StudentEnrollments
            .GetStudentEnrollmentsAsync(student.Id, currentTerm.Id);

        var todayClasses = new List<TodayScheduleItemDto>();

        foreach (var enrollment in enrollments.Where(e => e.Status == "Enrolled" || e.Status == "Active"))
        {
            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);

            // Get scheduled slots for this course offering
            var slots = await _unitOfWork.ScheduledSlots.GetByCourseOfferingAsync(offering.Id);

            var todaySlots = slots.Where(s => s.DayOfWeek.Equals(todayDayOfWeek, StringComparison.OrdinalIgnoreCase));

            foreach (var slot in todaySlots)
            {
                // Check if there's an actual session for today
                var sessions = await _unitOfWork.Sessions.GetByCourseOfferingAsync(offering.Id);
                var todaySession = sessions.FirstOrDefault(s => s.SessionDate.Date == today);

                // Get student's attendance for this session if exists
                string? attendanceStatus = null;
                if (todaySession != null)
                {
                    var attendances = await _unitOfWork.Attendances
                        .GetByStudentAndCourseOfferingAsync(student.Id, offering.Id);
                    var todayAttendance = attendances.FirstOrDefault(a => a.SessionId == todaySession.Id);
                    attendanceStatus = todayAttendance?.Status;
                }

                todayClasses.Add(new TodayScheduleItemDto
                {
                    SessionId = todaySession?.Id,
                    CourseOfferingId = offering.Id,
                    CourseCode = offering.Course.Code,
                    CourseName = offering.Course.Name,
                    TeacherName = offering.Teacher?.FullName ?? "TBA",
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    RoomName = slot.Room?.Name ?? todaySession?.Room?.Name,
                    RoomBuilding = slot.Room?.Building ?? todaySession?.Room?.Building,
                    SessionStatus = todaySession?.Status ?? "Scheduled",
                    AttendanceStatus = attendanceStatus
                });
            }
        }

        return Ok(new TodayScheduleDto
        {
            UserId = student.Id,
            UserName = student.FullName,
            UserType = "Student",
            StudentId = student.Id,
            StudentName = student.FullName,
            Date = today,
            DayName = todayDayOfWeek,
            Classes = todayClasses.OrderBy(c => c.StartTime).ToList()
        });
    }

    private async Task<ActionResult<UserScheduleDto>> GetStudentWeekSchedule(int studentId, int? termId)
    {
        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
        if (student == null)
            return NotFound(new { message = "Student record not found" });

        // Get current term if not specified
        if (!termId.HasValue)
        {
            var currentTerm = await _unitOfWork.Terms.GetCurrentTermAsync();
            termId = currentTerm?.Id;
        }

        // Get student enrollments for the term
        var enrollments = await _unitOfWork.StudentEnrollments
            .GetStudentEnrollmentsAsync(student.Id, termId);

        var scheduleItems = new List<ScheduleItemDto>();

        foreach (var enrollment in enrollments.Where(e => e.Status == "Enrolled" || e.Status == "Active"))
        {
            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);

            // Get scheduled slots for this course offering
            var slots = await _unitOfWork.ScheduledSlots.GetByCourseOfferingAsync(offering.Id);

            foreach (var slot in slots)
            {
                // Parse day of week from string
                if (Enum.TryParse<DayOfWeek>(slot.DayOfWeek, true, out var dayOfWeek))
                {
                    scheduleItems.Add(new ScheduleItemDto
                    {
                        SessionId = null,
                        CourseOfferingId = offering.Id,
                        CourseCode = offering.Course.Code,
                        CourseName = offering.Course.Name,
                        TeacherName = offering.Teacher?.FullName ?? "TBA",
                        DayOfWeek = dayOfWeek,
                        DayName = slot.DayOfWeek,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        RoomName = slot.Room?.Name,
                        RoomBuilding = slot.Room?.Building
                    });
                }
            }
        }

        var term = termId.HasValue
            ? await _unitOfWork.Terms.GetByIdAsync(termId.Value)
            : null;

        return Ok(new UserScheduleDto
        {
            UserId = student.Id,
            UserName = student.FullName,
            UserType = "Student",
            StudentId = student.Id,
            StudentName = student.FullName,
            TermId = termId,
            TermName = term?.Name,
            Schedule = scheduleItems.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList()
        });
    }

    private async Task<ActionResult<TodayScheduleDto>> GetStudentScheduleByDate(int studentId, DateTime date)
    {
        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
        if (student == null)
            return NotFound(new { message = "Student record not found" });

        var dayOfWeekString = date.DayOfWeek.ToString();

        // Get current term
        var currentTerm = await _unitOfWork.Terms.GetCurrentTermAsync();
        if (currentTerm == null)
            return Ok(new TodayScheduleDto
            {
                UserId = student.Id,
                UserName = student.FullName,
                UserType = "Student",
                StudentId = student.Id,
                StudentName = student.FullName,
                Date = date,
                DayName = dayOfWeekString,
                Classes = new List<TodayScheduleItemDto>()
            });

        // Get student enrollments
        var enrollments = await _unitOfWork.StudentEnrollments
            .GetStudentEnrollmentsAsync(student.Id, currentTerm.Id);

        var classes = new List<TodayScheduleItemDto>();

        foreach (var enrollment in enrollments.Where(e => e.Status == "Enrolled" || e.Status == "Active"))
        {
            var offering = await _unitOfWork.CourseOfferings
                .GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);

            // Get scheduled slots for this day of week
            var slots = await _unitOfWork.ScheduledSlots.GetByCourseOfferingAsync(offering.Id);
            var daySlots = slots.Where(s => s.DayOfWeek.Equals(dayOfWeekString, StringComparison.OrdinalIgnoreCase));

            foreach (var slot in daySlots)
            {
                // Check if there's an actual session for this date
                var sessions = await _unitOfWork.Sessions.GetByCourseOfferingAsync(offering.Id);
                var dateSession = sessions.FirstOrDefault(s => s.SessionDate.Date == date.Date);

                // Get student's attendance for this session if exists
                string? attendanceStatus = null;
                if (dateSession != null)
                {
                    var attendances = await _unitOfWork.Attendances
                        .GetByStudentAndCourseOfferingAsync(student.Id, offering.Id);
                    var sessionAttendance = attendances.FirstOrDefault(a => a.SessionId == dateSession.Id);
                    attendanceStatus = sessionAttendance?.Status;
                }

                classes.Add(new TodayScheduleItemDto
                {
                    SessionId = dateSession?.Id,
                    CourseOfferingId = offering.Id,
                    CourseCode = offering.Course.Code,
                    CourseName = offering.Course.Name,
                    TeacherName = offering.Teacher?.FullName ?? "TBA",
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    RoomName = slot.Room?.Name ?? dateSession?.Room?.Name,
                    RoomBuilding = slot.Room?.Building ?? dateSession?.Room?.Building,
                    SessionStatus = dateSession?.Status ?? "Scheduled",
                    AttendanceStatus = attendanceStatus
                });
            }
        }

        return Ok(new TodayScheduleDto
        {
            UserId = student.Id,
            UserName = student.FullName,
            UserType = "Student",
            StudentId = student.Id,
            StudentName = student.FullName,
            Date = date,
            DayName = dayOfWeekString,
            Classes = classes.OrderBy(c => c.StartTime).ToList()
        });
    }

    #endregion

    #region Teacher Schedule Methods

    private async Task<ActionResult<TodayScheduleDto>> GetTeacherTodaySchedule(int teacherId)
    {
        var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(teacherId);
        if (teacher == null)
            return NotFound(new { message = "Teacher record not found" });

        var today = DateTime.Today;
        var todayDayOfWeek = today.DayOfWeek.ToString();

        // Get current term
        var currentTerm = await _unitOfWork.Terms.GetCurrentTermAsync();
        if (currentTerm == null)
            return Ok(new TodayScheduleDto
            {
                UserId = teacher.Id,
                UserName = teacher.FullName,
                UserType = "Teacher",
                Date = today,
                DayName = todayDayOfWeek,
                Classes = new List<TodayScheduleItemDto>()
            });

        // Get teacher's course offerings for current term
        var courseOfferings = await _unitOfWork.CourseOfferings.GetByTeacherAsync(teacher.Id);
        var termOfferings = courseOfferings.Where(co => co.TermId == currentTerm.Id);

        var todayClasses = new List<TodayScheduleItemDto>();

        foreach (var offering in termOfferings)
        {
            var offeringDetails = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offering.Id);

            // Get scheduled slots for this course offering
            var slots = await _unitOfWork.ScheduledSlots.GetByCourseOfferingAsync(offering.Id);

            var todaySlots = slots.Where(s => s.DayOfWeek.Equals(todayDayOfWeek, StringComparison.OrdinalIgnoreCase));

            foreach (var slot in todaySlots)
            {
                // Check if there's an actual session for today
                var sessions = await _unitOfWork.Sessions.GetByCourseOfferingAsync(offering.Id);
                var todaySession = sessions.FirstOrDefault(s => s.SessionDate.Date == today);

                // Get teacher's attendance for this session if exists
                string? teacherAttendanceStatus = null;
                if (todaySession != null)
                {
                    var teacherAttendance = await _unitOfWork.TeacherAttendances
                        .GetByTeacherAndSessionAsync(teacher.Id, todaySession.Id);
                    if (teacherAttendance != null)
                    {
                        teacherAttendanceStatus = teacherAttendance.CheckOutTime.HasValue
                            ? "CheckedOut"
                            : "CheckedIn";
                    }
                }

                // Get enrollment count and checked-in count for this session
                var enrolledCount = await _unitOfWork.StudentEnrollments
                    .GetOfferingEnrollmentCountAsync(offering.Id);
                int? checkedInCount = null;
                if (todaySession != null)
                {
                    checkedInCount = await _unitOfWork.Attendances.GetCheckedInCountAsync(todaySession.Id);
                }

                todayClasses.Add(new TodayScheduleItemDto
                {
                    SessionId = todaySession?.Id,
                    CourseOfferingId = offering.Id,
                    CourseCode = offeringDetails.Course.Code,
                    CourseName = offeringDetails.Course.Name,
                    TeacherName = teacher.FullName,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    RoomName = slot.Room?.Name ?? todaySession?.Room?.Name,
                    RoomBuilding = slot.Room?.Building ?? todaySession?.Room?.Building,
                    SessionStatus = todaySession?.Status ?? "Scheduled",
                    TeacherAttendanceStatus = teacherAttendanceStatus,
                    EnrolledCount = enrolledCount,
                    CheckedInCount = checkedInCount
                });
            }
        }

        return Ok(new TodayScheduleDto
        {
            UserId = teacher.Id,
            UserName = teacher.FullName,
            UserType = "Teacher",
            Date = today,
            DayName = todayDayOfWeek,
            Classes = todayClasses.OrderBy(c => c.StartTime).ToList()
        });
    }

    private async Task<ActionResult<UserScheduleDto>> GetTeacherWeekSchedule(int teacherId, int? termId)
    {
        var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(teacherId);
        if (teacher == null)
            return NotFound(new { message = "Teacher record not found" });

        // Get current term if not specified
        if (!termId.HasValue)
        {
            var currentTerm = await _unitOfWork.Terms.GetCurrentTermAsync();
            termId = currentTerm?.Id;
        }

        // Get teacher's course offerings for the term
        var courseOfferings = await _unitOfWork.CourseOfferings.GetByTeacherAsync(teacher.Id);
        var termOfferings = termId.HasValue
            ? courseOfferings.Where(co => co.TermId == termId.Value)
            : courseOfferings;

        var scheduleItems = new List<ScheduleItemDto>();

        foreach (var offering in termOfferings)
        {
            var offeringDetails = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offering.Id);

            // Get scheduled slots for this course offering
            var slots = await _unitOfWork.ScheduledSlots.GetByCourseOfferingAsync(offering.Id);

            // Get enrollment count for this offering
            var enrolledCount = await _unitOfWork.StudentEnrollments
                .GetOfferingEnrollmentCountAsync(offering.Id);

            foreach (var slot in slots)
            {
                // Parse day of week from string
                if (Enum.TryParse<DayOfWeek>(slot.DayOfWeek, true, out var dayOfWeek))
                {
                    scheduleItems.Add(new ScheduleItemDto
                    {
                        SessionId = null,
                        CourseOfferingId = offering.Id,
                        CourseCode = offeringDetails.Course.Code,
                        CourseName = offeringDetails.Course.Name,
                        TeacherName = teacher.FullName,
                        DayOfWeek = dayOfWeek,
                        DayName = slot.DayOfWeek,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        RoomName = slot.Room?.Name,
                        RoomBuilding = slot.Room?.Building,
                        EnrolledCount = enrolledCount
                    });
                }
            }
        }

        var term = termId.HasValue
            ? await _unitOfWork.Terms.GetByIdAsync(termId.Value)
            : null;

        return Ok(new UserScheduleDto
        {
            UserId = teacher.Id,
            UserName = teacher.FullName,
            UserType = "Teacher",
            TermId = termId,
            TermName = term?.Name,
            Schedule = scheduleItems.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).ToList()
        });
    }

    private async Task<ActionResult<TodayScheduleDto>> GetTeacherScheduleByDate(int teacherId, DateTime date)
    {
        var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(teacherId);
        if (teacher == null)
            return NotFound(new { message = "Teacher record not found" });

        var dayOfWeekString = date.DayOfWeek.ToString();

        // Get current term
        var currentTerm = await _unitOfWork.Terms.GetCurrentTermAsync();
        if (currentTerm == null)
            return Ok(new TodayScheduleDto
            {
                UserId = teacher.Id,
                UserName = teacher.FullName,
                UserType = "Teacher",
                Date = date,
                DayName = dayOfWeekString,
                Classes = new List<TodayScheduleItemDto>()
            });

        // Get teacher's course offerings for current term
        var courseOfferings = await _unitOfWork.CourseOfferings.GetByTeacherAsync(teacher.Id);
        var termOfferings = courseOfferings.Where(co => co.TermId == currentTerm.Id);

        var classes = new List<TodayScheduleItemDto>();

        foreach (var offering in termOfferings)
        {
            var offeringDetails = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offering.Id);

            // Get scheduled slots for this day of week
            var slots = await _unitOfWork.ScheduledSlots.GetByCourseOfferingAsync(offering.Id);
            var daySlots = slots.Where(s => s.DayOfWeek.Equals(dayOfWeekString, StringComparison.OrdinalIgnoreCase));

            foreach (var slot in daySlots)
            {
                // Check if there's an actual session for this date
                var sessions = await _unitOfWork.Sessions.GetByCourseOfferingAsync(offering.Id);
                var dateSession = sessions.FirstOrDefault(s => s.SessionDate.Date == date.Date);

                // Get teacher's attendance for this session if exists
                string? teacherAttendanceStatus = null;
                if (dateSession != null)
                {
                    var teacherAttendance = await _unitOfWork.TeacherAttendances
                        .GetByTeacherAndSessionAsync(teacher.Id, dateSession.Id);
                    if (teacherAttendance != null)
                    {
                        teacherAttendanceStatus = teacherAttendance.CheckOutTime.HasValue
                            ? "CheckedOut"
                            : "CheckedIn";
                    }
                }

                // Get enrollment count and checked-in count
                var enrolledCount = await _unitOfWork.StudentEnrollments
                    .GetOfferingEnrollmentCountAsync(offering.Id);
                int? checkedInCount = null;
                if (dateSession != null)
                {
                    checkedInCount = await _unitOfWork.Attendances.GetCheckedInCountAsync(dateSession.Id);
                }

                classes.Add(new TodayScheduleItemDto
                {
                    SessionId = dateSession?.Id,
                    CourseOfferingId = offering.Id,
                    CourseCode = offeringDetails.Course.Code,
                    CourseName = offeringDetails.Course.Name,
                    TeacherName = teacher.FullName,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    RoomName = slot.Room?.Name ?? dateSession?.Room?.Name,
                    RoomBuilding = slot.Room?.Building ?? dateSession?.Room?.Building,
                    SessionStatus = dateSession?.Status ?? "Scheduled",
                    TeacherAttendanceStatus = teacherAttendanceStatus,
                    EnrolledCount = enrolledCount,
                    CheckedInCount = checkedInCount
                });
            }
        }

        return Ok(new TodayScheduleDto
        {
            UserId = teacher.Id,
            UserName = teacher.FullName,
            UserType = "Teacher",
            Date = date,
            DayName = dayOfWeekString,
            Classes = classes.OrderBy(c => c.StartTime).ToList()
        });
    }

    #endregion

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }
}
