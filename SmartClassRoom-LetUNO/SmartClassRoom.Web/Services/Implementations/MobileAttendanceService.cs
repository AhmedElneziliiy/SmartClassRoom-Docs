using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartClassRoom.Web.Configuration;
using SmartClassRoom.Web.Data;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.DTOs.MobileAttendance;
using SmartClassRoom.Web.Models.Entities.Attendance;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service implementation for mobile attendance operations
/// </summary>
public class MobileAttendanceService : IMobileAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly AttendanceSettings _settings;
    private readonly ILogger<MobileAttendanceService> _logger;

    public MobileAttendanceService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IOptions<AttendanceSettings> settings,
        ILogger<MobileAttendanceService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<CheckInResponse> CheckInAsync(int userId, CheckInRequest request)
    {
        // 1. Get the user and validate they are a student
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new CheckInResponse
            {
                Success = false,
                Message = "User not found.",
                ErrorCode = "USER_NOT_FOUND"
            };
        }

        // 2. Validate UDID (strict match)
        if (string.IsNullOrEmpty(user.UDID) || user.UDID != request.UDID)
        {
            return new CheckInResponse
            {
                Success = false,
                Message = "Device not authorized. UDID does not match.",
                ErrorCode = "UDID_MISMATCH"
            };
        }

        // 3. Validate user is a student
        if (user.UserType != "Student")
        {
            return new CheckInResponse
            {
                Success = false,
                Message = "Only students can check in to sessions.",
                ErrorCode = "NOT_A_STUDENT"
            };
        }

        // 4. Get the session with related data
        var session = await _context.Sessions
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId);

        if (session == null)
        {
            return new CheckInResponse
            {
                Success = false,
                Message = "Session not found.",
                ErrorCode = "SESSION_NOT_FOUND"
            };
        }

        // 5. Validate session status - must be InProgress (teacher started it)
        if (session.Status != "InProgress")
        {
            return new CheckInResponse
            {
                Success = false,
                Message = "Session has not been started by the teacher yet.",
                ErrorCode = "SESSION_NOT_STARTED"
            };
        }

        // 6. Check enrollment in the course offering
        var enrollment = await _unitOfWork.StudentEnrollments
            .GetEnrollmentAsync(userId, session.CourseOfferingId);

        if (enrollment == null)
        {
            // Log detailed information for debugging
            _logger.LogWarning(
                "Check-in failed: No enrollment record found. StudentId={StudentId}, SessionId={SessionId}, CourseOfferingId={CourseOfferingId}, CourseName={CourseName}",
                userId, request.SessionId, session.CourseOfferingId, session.CourseOffering?.Course?.Name ?? "Unknown");

            return new CheckInResponse
            {
                Success = false,
                Message = $"No enrollment record found for course offering ID {session.CourseOfferingId} ({session.CourseOffering?.Course?.Name ?? "Unknown Course"}). Please contact administrator.",
                ErrorCode = "NO_ENROLLMENT_RECORD"
            };
        }

        if (enrollment.Status != "Enrolled")
        {
            _logger.LogWarning(
                "Check-in failed: Invalid enrollment status. StudentId={StudentId}, SessionId={SessionId}, EnrollmentId={EnrollmentId}, Status={Status}, CourseOfferingId={CourseOfferingId}",
                userId, request.SessionId, enrollment.Id, enrollment.Status, session.CourseOfferingId);

            return new CheckInResponse
            {
                Success = false,
                Message = $"Your enrollment status is '{enrollment.Status}'. Only students with 'Enrolled' status can check in.",
                ErrorCode = "INVALID_ENROLLMENT_STATUS"
            };
        }

        // 7. Check for existing attendance record
        var existingAttendance = await _unitOfWork.Attendances
            .GetByStudentAndSessionAsync(userId, request.SessionId);

        if (existingAttendance != null)
        {
            // Already checked in
            if (existingAttendance.CheckOutTime.HasValue)
            {
                // Already checked out - no re-entry allowed
                return new CheckInResponse
                {
                    Success = false,
                    Message = "You have already checked out. Re-entry is not allowed.",
                    ErrorCode = "ALREADY_CHECKED_OUT"
                };
            }
            else
            {
                // Still checked in
                return new CheckInResponse
                {
                    Success = false,
                    Message = "You are already checked in to this session.",
                    ErrorCode = "ALREADY_CHECKED_IN"
                };
            }
        }

        // 8. Validate device proximity (if required)
        if (_settings.RequireDeviceProximity)
        {
            if (session.Room == null || string.IsNullOrEmpty(session.Room.DeviceId))
            {
                return new CheckInResponse
                {
                    Success = false,
                    Message = "Room has no registered device for proximity check.",
                    ErrorCode = "ROOM_NO_DEVICE"
                };
            }

            if (!request.DeviceIds.Contains(session.Room.DeviceId))
            {
                return new CheckInResponse
                {
                    Success = false,
                    Message = "You are not within range of the classroom device.",
                    ErrorCode = "DEVICE_NOT_IN_RANGE"
                };
            }
        }

        // 9. Determine attendance status (Present or Late)
        var now = DateTime.UtcNow;
        var sessionStartDateTime = session.StartedAt ?? session.SessionDate.Date.Add(session.StartTime);
        var lateThreshold = sessionStartDateTime.AddMinutes(_settings.LateGracePeriodMinutes);

        var status = now > lateThreshold ? "Late" : "Present";

        // 10. Create attendance record
        var attendance = new Attendance
        {
            SessionId = request.SessionId,
            StudentId = userId,
            CheckInTime = now,
            Status = status,
            VerificationMethod = "ESP32"
        };

        await _unitOfWork.Attendances.AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();

        return new CheckInResponse
        {
            Success = true,
            Message = status == "Late"
                ? $"Check-in successful. You are marked as late (arrived after {_settings.LateGracePeriodMinutes} minutes grace period)."
                : "Check-in successful.",
            Data = new CheckInData
            {
                AttendanceId = attendance.Id,
                SessionId = session.Id,
                SessionName = session.Topic ?? $"Session {session.Id}",
                CourseName = session.CourseOffering?.Course?.Name ?? "Unknown Course",
                RoomName = session.Room?.Name ?? session.Room?.Number ?? "Unknown Room",
                CheckInTime = attendance.CheckInTime,
                Status = status,
                IsLate = status == "Late"
            }
        };
    }

    public async Task<CheckOutResponse> CheckOutAsync(int userId, CheckOutRequest request)
    {
        // 1. Get the user and validate UDID
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new CheckOutResponse
            {
                Success = false,
                Message = "User not found.",
                ErrorCode = "USER_NOT_FOUND"
            };
        }

        // 2. Validate UDID (strict match)
        if (string.IsNullOrEmpty(user.UDID) || user.UDID != request.UDID)
        {
            return new CheckOutResponse
            {
                Success = false,
                Message = "Device not authorized. UDID does not match.",
                ErrorCode = "UDID_MISMATCH"
            };
        }

        // 3. Get existing attendance record
        var attendance = await _unitOfWork.Attendances
            .GetByStudentAndSessionAsync(userId, request.SessionId);

        if (attendance == null)
        {
            return new CheckOutResponse
            {
                Success = false,
                Message = "You have not checked in to this session.",
                ErrorCode = "NOT_CHECKED_IN"
            };
        }

        // 4. Check if already checked out
        if (attendance.CheckOutTime.HasValue)
        {
            return new CheckOutResponse
            {
                Success = false,
                Message = "You have already checked out from this session.",
                ErrorCode = "ALREADY_CHECKED_OUT"
            };
        }

        // 5. Optional: Validate device proximity for checkout
        if (_settings.RequireDeviceProximityForCheckout && request.DeviceIds != null && request.DeviceIds.Any())
        {
            var session = attendance.Session;
            if (session?.Room != null && !string.IsNullOrEmpty(session.Room.DeviceId))
            {
                if (!request.DeviceIds.Contains(session.Room.DeviceId))
                {
                    return new CheckOutResponse
                    {
                        Success = false,
                        Message = "You are not within range of the classroom device.",
                        ErrorCode = "DEVICE_NOT_IN_RANGE"
                    };
                }
            }
        }

        // 6. Record checkout time
        var now = DateTime.UtcNow;
        attendance.CheckOutTime = now;

        _unitOfWork.Attendances.Update(attendance);
        await _unitOfWork.SaveChangesAsync();

        // Calculate duration
        var duration = now - attendance.CheckInTime;
        var durationString = $"{(int)duration.TotalHours}h {duration.Minutes}m";

        return new CheckOutResponse
        {
            Success = true,
            Message = "Check-out successful.",
            Data = new CheckOutData
            {
                AttendanceId = attendance.Id,
                SessionId = attendance.SessionId,
                SessionName = attendance.Session?.Topic ?? $"Session {attendance.SessionId}",
                CourseName = attendance.Session?.CourseOffering?.Course?.Name ?? "Unknown Course",
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = now,
                Duration = durationString,
                Status = attendance.Status ?? "Present"
            }
        };
    }

    public async Task<SessionManagementResponse> StartSessionAsync(int userId, SessionManagementRequest request)
    {
        // 1. Get the user
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "User not found.",
                ErrorCode = "USER_NOT_FOUND"
            };
        }

        // 2. Optional UDID validation
        if (!string.IsNullOrEmpty(request.UDID))
        {
            if (string.IsNullOrEmpty(user.UDID) || user.UDID != request.UDID)
            {
                return new SessionManagementResponse
                {
                    Success = false,
                    Message = "Device not authorized. UDID does not match.",
                    ErrorCode = "UDID_MISMATCH"
                };
            }
        }

        // 3. Get the session
        var session = await _context.Sessions
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId);

        if (session == null)
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "Session not found.",
                ErrorCode = "SESSION_NOT_FOUND"
            };
        }

        // 4. Check authorization - Admin or Teacher who owns this session
        var isAdmin = user.UserType == "Admin" || await _userManager.IsInRoleAsync(user, "Admin");
        var isSessionTeacher = user.UserType == "Teacher" && session.CourseOffering.TeacherId == userId;

        if (!isAdmin && !isSessionTeacher)
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "You are not authorized to manage this session.",
                ErrorCode = "NOT_AUTHORIZED"
            };
        }

        // 5. Check current status
        if (session.Status == "InProgress")
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "Session is already in progress.",
                ErrorCode = "INVALID_STATUS"
            };
        }

        if (session.Status == "Completed")
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "Session has already been completed.",
                ErrorCode = "INVALID_STATUS"
            };
        }

        if (session.Status == "Cancelled")
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "Session has been cancelled.",
                ErrorCode = "INVALID_STATUS"
            };
        }

        // 6. Validate device proximity for teacher (if teacher and room has device)
        if (isSessionTeacher && _settings.RequireDeviceProximity)
        {
            if (session.Room != null && !string.IsNullOrEmpty(session.Room.DeviceId))
            {
                if (request.DeviceIds == null || !request.DeviceIds.Contains(session.Room.DeviceId))
                {
                    return new SessionManagementResponse
                    {
                        Success = false,
                        Message = "You are not within range of the classroom device. Please ensure you are in the classroom.",
                        ErrorCode = "DEVICE_NOT_IN_RANGE"
                    };
                }
            }
        }

        // 7. Start the session
        var now = DateTime.UtcNow;
        session.Status = "InProgress";
        session.StartedAt = now;

        _context.Sessions.Update(session);

        // 8. Create teacher attendance record (if teacher)
        if (isSessionTeacher)
        {
            var teacherAttendance = new TeacherAttendance
            {
                SessionId = session.Id,
                TeacherId = userId,
                CheckInTime = now,
                Status = "Present",
                VerificationMethod = request.DeviceIds?.Any() == true ? "ESP32" : "Manual"
            };

            await _unitOfWork.TeacherAttendances.AddAsync(teacherAttendance);
        }

        await _context.SaveChangesAsync();
        await _unitOfWork.SaveChangesAsync();

        // Get enrollment count
        var enrolledCount = await _unitOfWork.StudentEnrollments
            .GetOfferingEnrollmentCountAsync(session.CourseOfferingId);

        return new SessionManagementResponse
        {
            Success = true,
            Message = isSessionTeacher
                ? "Session started successfully. You are checked in. Students can now check in."
                : "Session started successfully. Students can now check in.",
            Data = new SessionManagementData
            {
                SessionId = session.Id,
                SessionName = session.Topic ?? $"Session {session.Id}",
                CourseName = session.CourseOffering?.Course?.Name ?? "Unknown Course",
                RoomName = session.Room?.Name ?? session.Room?.Number ?? "Unknown Room",
                Status = session.Status,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                EnrolledCount = enrolledCount,
                CheckedInCount = 0
            }
        };
    }

    public async Task<SessionManagementResponse> EndSessionAsync(int userId, SessionManagementRequest request)
    {
        // 1. Get the user
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "User not found.",
                ErrorCode = "USER_NOT_FOUND"
            };
        }

        // 2. Optional UDID validation
        if (!string.IsNullOrEmpty(request.UDID))
        {
            if (string.IsNullOrEmpty(user.UDID) || user.UDID != request.UDID)
            {
                return new SessionManagementResponse
                {
                    Success = false,
                    Message = "Device not authorized. UDID does not match.",
                    ErrorCode = "UDID_MISMATCH"
                };
            }
        }

        // 3. Get the session
        var session = await _context.Sessions
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId);

        if (session == null)
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "Session not found.",
                ErrorCode = "SESSION_NOT_FOUND"
            };
        }

        // 4. Check authorization - Admin or Teacher who owns this session
        var isAdmin = user.UserType == "Admin" || await _userManager.IsInRoleAsync(user, "Admin");
        var isSessionTeacher = user.UserType == "Teacher" && session.CourseOffering.TeacherId == userId;

        if (!isAdmin && !isSessionTeacher)
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = "You are not authorized to manage this session.",
                ErrorCode = "NOT_AUTHORIZED"
            };
        }

        // 5. Check current status
        if (session.Status != "InProgress")
        {
            return new SessionManagementResponse
            {
                Success = false,
                Message = $"Session is not in progress. Current status: {session.Status}",
                ErrorCode = "INVALID_STATUS"
            };
        }

        // 6. Validate device proximity for teacher (if teacher and room has device)
        if (isSessionTeacher && _settings.RequireDeviceProximity)
        {
            if (session.Room != null && !string.IsNullOrEmpty(session.Room.DeviceId))
            {
                if (request.DeviceIds == null || !request.DeviceIds.Contains(session.Room.DeviceId))
                {
                    return new SessionManagementResponse
                    {
                        Success = false,
                        Message = "You are not within range of the classroom device. Please ensure you are in the classroom.",
                        ErrorCode = "DEVICE_NOT_IN_RANGE"
                    };
                }
            }
        }

        // 7. End the session
        var now = DateTime.UtcNow;
        session.Status = "Completed";
        session.EndedAt = now;

        _context.Sessions.Update(session);

        // 8. Update teacher attendance record (if teacher)
        if (isSessionTeacher)
        {
            var teacherAttendance = await _unitOfWork.TeacherAttendances
                .GetByTeacherAndSessionAsync(userId, session.Id);

            if (teacherAttendance != null)
            {
                teacherAttendance.CheckOutTime = now;
                _unitOfWork.TeacherAttendances.Update(teacherAttendance);
            }
        }

        // 9. Force checkout all students who haven't checked out
        var studentsForceCheckedOut = await ForceCheckoutAllStudentsAsync(session.Id, now);

        await _context.SaveChangesAsync();
        await _unitOfWork.SaveChangesAsync();

        // Get counts
        var enrolledCount = await _unitOfWork.StudentEnrollments
            .GetOfferingEnrollmentCountAsync(session.CourseOfferingId);
        var checkedInCount = await _unitOfWork.Attendances
            .GetCheckedInCountAsync(session.Id);

        var message = isSessionTeacher
            ? "Session ended successfully. You are checked out."
            : "Session ended successfully.";

        if (studentsForceCheckedOut > 0)
        {
            message += $" {studentsForceCheckedOut} student(s) were automatically checked out.";
        }

        return new SessionManagementResponse
        {
            Success = true,
            Message = message,
            Data = new SessionManagementData
            {
                SessionId = session.Id,
                SessionName = session.Topic ?? $"Session {session.Id}",
                CourseName = session.CourseOffering?.Course?.Name ?? "Unknown Course",
                RoomName = session.Room?.Name ?? session.Room?.Number ?? "Unknown Room",
                Status = session.Status,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                EnrolledCount = enrolledCount,
                CheckedInCount = checkedInCount
            }
        };
    }

    public async Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsForStudentAsync(int studentId)
    {
        // Get student's enrollments
        var enrollments = await _unitOfWork.StudentEnrollments
            .GetStudentEnrollmentsAsync(studentId);

        var enrolledOfferingIds = enrollments
            .Where(e => e.Status == "Enrolled")
            .Select(e => e.CourseOfferingId)
            .ToList();

        if (!enrolledOfferingIds.Any())
        {
            return Enumerable.Empty<ActiveSessionDto>();
        }

        // Get sessions that are Scheduled or InProgress for enrolled courses
        var today = DateTime.UtcNow.Date;
        var sessions = await _context.Sessions
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.Room)
            .Where(s => enrolledOfferingIds.Contains(s.CourseOfferingId) &&
                       (s.Status == "Scheduled" || s.Status == "InProgress") &&
                       s.SessionDate >= today)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        var result = new List<ActiveSessionDto>();

        foreach (var session in sessions)
        {
            // Check if student already has attendance for this session
            var attendance = await _unitOfWork.Attendances
                .GetByStudentAndSessionAsync(studentId, session.Id);

            result.Add(new ActiveSessionDto
            {
                SessionId = session.Id,
                CourseName = session.CourseOffering?.Course?.Name ?? "Unknown Course",
                CourseCode = session.CourseOffering?.Course?.Code ?? "",
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                RoomName = session.Room?.Name ?? session.Room?.Number ?? "TBD",
                RoomDeviceId = session.Room?.DeviceId,
                Status = session.Status ?? "Scheduled",
                IsStarted = session.Status == "InProgress",
                CanCheckIn = session.Status == "InProgress" &&
                            (attendance == null || (!attendance.CheckOutTime.HasValue && attendance.CheckInTime == default)),
                HasCheckedIn = attendance != null && attendance.CheckInTime != default,
                HasCheckedOut = attendance?.CheckOutTime.HasValue ?? false,
                CheckInTime = attendance?.CheckInTime,
                CheckOutTime = attendance?.CheckOutTime,
                AttendanceStatus = attendance?.Status
            });
        }

        return result;
    }

    public async Task<IEnumerable<ActiveSessionDto>> GetManageableSessionsForTeacherAsync(int teacherId)
    {
        var today = DateTime.UtcNow.Date;

        // Get sessions for courses taught by this teacher
        var sessions = await _context.Sessions
            .Include(s => s.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(s => s.Room)
            .Where(s => s.CourseOffering.TeacherId == teacherId &&
                       (s.Status == "Scheduled" || s.Status == "InProgress") &&
                       s.SessionDate >= today)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        var result = new List<ActiveSessionDto>();

        foreach (var session in sessions)
        {
            var enrolledCount = await _unitOfWork.StudentEnrollments
                .GetOfferingEnrollmentCountAsync(session.CourseOfferingId);
            var checkedInCount = await _unitOfWork.Attendances
                .GetCheckedInCountAsync(session.Id);

            // Get teacher attendance status
            var teacherAttendance = await _unitOfWork.TeacherAttendances
                .GetByTeacherAndSessionAsync(teacherId, session.Id);

            result.Add(new ActiveSessionDto
            {
                SessionId = session.Id,
                CourseName = session.CourseOffering?.Course?.Name ?? "Unknown Course",
                CourseCode = session.CourseOffering?.Course?.Code ?? "",
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                RoomName = session.Room?.Name ?? session.Room?.Number ?? "TBD",
                RoomDeviceId = session.Room?.DeviceId,
                Status = session.Status ?? "Scheduled",
                IsStarted = session.Status == "InProgress",
                CanCheckIn = false, // Teachers don't check in separately - they start session
                HasCheckedIn = teacherAttendance != null,
                HasCheckedOut = teacherAttendance?.CheckOutTime.HasValue ?? false,
                CheckInTime = teacherAttendance?.CheckInTime,
                CheckOutTime = teacherAttendance?.CheckOutTime,
                EnrolledCount = enrolledCount,
                CheckedInCount = checkedInCount
            });
        }

        return result;
    }

    public async Task<AttendanceStatusDto?> GetAttendanceStatusAsync(int studentId, int sessionId)
    {
        var attendance = await _unitOfWork.Attendances
            .GetByStudentAndSessionAsync(studentId, sessionId);

        if (attendance == null)
        {
            return null;
        }

        string? duration = null;
        if (attendance.CheckOutTime.HasValue)
        {
            var dur = attendance.CheckOutTime.Value - attendance.CheckInTime;
            duration = $"{(int)dur.TotalHours}h {dur.Minutes}m";
        }

        return new AttendanceStatusDto
        {
            AttendanceId = attendance.Id,
            SessionId = attendance.SessionId,
            SessionName = attendance.Session?.Topic ?? $"Session {attendance.SessionId}",
            CourseName = attendance.Session?.CourseOffering?.Course?.Name ?? "Unknown Course",
            Status = attendance.Status ?? "Present",
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = attendance.CheckOutTime,
            IsCheckedOut = attendance.CheckOutTime.HasValue,
            Duration = duration
        };
    }

    public async Task<TeacherAttendanceStatusDto?> GetTeacherAttendanceStatusAsync(int teacherId, int sessionId)
    {
        var attendance = await _unitOfWork.TeacherAttendances
            .GetByTeacherAndSessionAsync(teacherId, sessionId);

        if (attendance == null)
        {
            return null;
        }

        string? duration = null;
        if (attendance.CheckOutTime.HasValue)
        {
            var dur = attendance.CheckOutTime.Value - attendance.CheckInTime;
            duration = $"{(int)dur.TotalHours}h {dur.Minutes}m";
        }

        // Get session details for counts
        var enrolledCount = await _unitOfWork.StudentEnrollments
            .GetOfferingEnrollmentCountAsync(attendance.Session.CourseOfferingId);
        var checkedInCount = await _unitOfWork.Attendances
            .GetCheckedInCountAsync(sessionId);

        return new TeacherAttendanceStatusDto
        {
            AttendanceId = attendance.Id,
            SessionId = attendance.SessionId,
            SessionName = attendance.Session?.Topic ?? $"Session {attendance.SessionId}",
            CourseName = attendance.Session?.CourseOffering?.Course?.Name ?? "Unknown Course",
            Status = attendance.Status ?? "Present",
            CheckInTime = attendance.CheckInTime,
            CheckOutTime = attendance.CheckOutTime,
            IsCheckedOut = attendance.CheckOutTime.HasValue,
            Duration = duration,
            EnrolledCount = enrolledCount,
            CheckedInCount = checkedInCount
        };
    }

    public async Task<int> ForceCheckoutSessionAsync(int sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.CourseOffering)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var forceCheckoutCount = 0;

        // Force checkout teacher if not already checked out
        var teacherAttendance = await _unitOfWork.TeacherAttendances
            .GetBySessionAsync(sessionId);

        if (teacherAttendance != null && !teacherAttendance.CheckOutTime.HasValue)
        {
            teacherAttendance.CheckOutTime = now;
            teacherAttendance.Status = "ForceCheckout";
            teacherAttendance.Notes = "Automatically checked out due to grace period expiration";
            _unitOfWork.TeacherAttendances.Update(teacherAttendance);
            forceCheckoutCount++;
        }

        // Force checkout all students
        forceCheckoutCount += await ForceCheckoutAllStudentsAsync(sessionId, now);

        // Update session status if still in progress
        if (session.Status == "InProgress")
        {
            session.Status = "Completed";
            session.EndedAt = now;
            _context.Sessions.Update(session);
        }

        await _context.SaveChangesAsync();
        await _unitOfWork.SaveChangesAsync();

        return forceCheckoutCount;
    }

    private async Task<int> ForceCheckoutAllStudentsAsync(int sessionId, DateTime checkoutTime)
    {
        // Get all students who haven't checked out
        var uncheckedOutAttendances = await _context.Set<Attendance>()
            .Where(a => a.SessionId == sessionId && !a.CheckOutTime.HasValue)
            .ToListAsync();

        foreach (var attendance in uncheckedOutAttendances)
        {
            attendance.CheckOutTime = checkoutTime;
            attendance.Notes = string.IsNullOrEmpty(attendance.Notes)
                ? "Automatically checked out when session ended"
                : attendance.Notes + "; Automatically checked out when session ended";
        }

        return uncheckedOutAttendances.Count;
    }
}
