using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Models.DTOs.MobileAttendance;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

/// <summary>
/// API Controller for mobile attendance operations (check-in, check-out, session management)
/// </summary>
[Route("api/mobile/attendance")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MobileAttendanceController : ControllerBase
{
    private readonly IMobileAttendanceService _attendanceService;

    public MobileAttendanceController(IMobileAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>
    /// Get the current user ID from JWT claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Student check-in to a session
    /// Validates UDID, device proximity, enrollment, and session status
    /// </summary>
    /// <param name="request">Check-in request with UDID, SessionId, and DeviceIds</param>
    /// <returns>Check-in result with attendance details</returns>
    [HttpPost("check-in")]
    public async Task<ActionResult<CheckInResponse>> CheckIn([FromBody] CheckInRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new CheckInResponse
            {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = "INVALID_REQUEST"
            });
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new CheckInResponse
            {
                Success = false,
                Message = "User not authenticated.",
                ErrorCode = "NOT_AUTHENTICATED"
            });
        }

        var response = await _attendanceService.CheckInAsync(userId, request);

        if (!response.Success)
        {
            return response.ErrorCode switch
            {
                "UDID_MISMATCH" => StatusCode(403, response),
                "NOT_ENROLLED" => StatusCode(403, response),
                "NO_ENROLLMENT_RECORD" => StatusCode(403, response),
                "INVALID_ENROLLMENT_STATUS" => StatusCode(403, response),
                "NOT_AUTHORIZED" => StatusCode(403, response),
                "ALREADY_CHECKED_IN" => Conflict(response),
                "ALREADY_CHECKED_OUT" => Conflict(response),
                _ => BadRequest(response)
            };
        }

        return Ok(response);
    }

    /// <summary>
    /// Student check-out from a session
    /// Validates UDID and existing attendance record
    /// </summary>
    /// <param name="request">Check-out request with UDID and SessionId</param>
    /// <returns>Check-out result with duration details</returns>
    [HttpPost("check-out")]
    public async Task<ActionResult<CheckOutResponse>> CheckOut([FromBody] CheckOutRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new CheckOutResponse
            {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = "INVALID_REQUEST"
            });
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new CheckOutResponse
            {
                Success = false,
                Message = "User not authenticated.",
                ErrorCode = "NOT_AUTHENTICATED"
            });
        }

        var response = await _attendanceService.CheckOutAsync(userId, request);

        if (!response.Success)
        {
            return response.ErrorCode switch
            {
                "UDID_MISMATCH" => StatusCode(403, response),
                "ALREADY_CHECKED_OUT" => Conflict(response),
                _ => BadRequest(response)
            };
        }

        return Ok(response);
    }

    /// <summary>
    /// Get active/upcoming sessions for the current student
    /// Returns sessions where the student is enrolled with check-in status
    /// </summary>
    /// <returns>List of active sessions</returns>
    [HttpGet("active-sessions")]
    public async Task<ActionResult<IEnumerable<ActiveSessionDto>>> GetActiveSessions()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new { success = false, message = "User not authenticated." });
        }

        var sessions = await _attendanceService.GetActiveSessionsForStudentAsync(userId);
        return Ok(new
        {
            success = true,
            data = sessions
        });
    }

    /// <summary>
    /// Get attendance status for a specific session
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>Attendance status or null if not checked in</returns>
    [HttpGet("status/{sessionId}")]
    public async Task<ActionResult<AttendanceStatusDto>> GetAttendanceStatus(int sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new { success = false, message = "User not authenticated." });
        }

        var status = await _attendanceService.GetAttendanceStatusAsync(userId, sessionId);

        if (status == null)
        {
            return Ok(new
            {
                success = true,
                message = "No attendance record found for this session.",
                data = (AttendanceStatusDto?)null
            });
        }

        return Ok(new
        {
            success = true,
            data = status
        });
    }

    /// <summary>
    /// Start a session (Teacher or Admin only)
    /// Changes session status from Scheduled to InProgress
    /// </summary>
    /// <param name="request">Session management request with SessionId</param>
    /// <returns>Session management result</returns>
    [HttpPost("session/start")]
    public async Task<ActionResult<SessionManagementResponse>> StartSession([FromBody] SessionManagementRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new SessionManagementResponse
            {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = "INVALID_REQUEST"
            });
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new SessionManagementResponse
            {
                Success = false,
                Message = "User not authenticated.",
                ErrorCode = "NOT_AUTHENTICATED"
            });
        }

        var response = await _attendanceService.StartSessionAsync(userId, request);

        if (!response.Success)
        {
            return response.ErrorCode switch
            {
                "UDID_MISMATCH" => StatusCode(403, response),
                "NOT_AUTHORIZED" => StatusCode(403, response),
                "INVALID_STATUS" => Conflict(response),
                _ => BadRequest(response)
            };
        }

        return Ok(response);
    }

    /// <summary>
    /// End a session (Teacher or Admin only)
    /// Changes session status from InProgress to Completed
    /// </summary>
    /// <param name="request">Session management request with SessionId</param>
    /// <returns>Session management result</returns>
    [HttpPost("session/end")]
    public async Task<ActionResult<SessionManagementResponse>> EndSession([FromBody] SessionManagementRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new SessionManagementResponse
            {
                Success = false,
                Message = "Invalid request data.",
                ErrorCode = "INVALID_REQUEST"
            });
        }

        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new SessionManagementResponse
            {
                Success = false,
                Message = "User not authenticated.",
                ErrorCode = "NOT_AUTHENTICATED"
            });
        }

        var response = await _attendanceService.EndSessionAsync(userId, request);

        if (!response.Success)
        {
            return response.ErrorCode switch
            {
                "UDID_MISMATCH" => StatusCode(403, response),
                "NOT_AUTHORIZED" => StatusCode(403, response),
                "INVALID_STATUS" => Conflict(response),
                _ => BadRequest(response)
            };
        }

        return Ok(response);
    }

    /// <summary>
    /// Get sessions that the current teacher can manage (start/end)
    /// </summary>
    /// <returns>List of manageable sessions</returns>
    [HttpGet("teacher/sessions")]
    public async Task<ActionResult<IEnumerable<ActiveSessionDto>>> GetTeacherSessions()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new { success = false, message = "User not authenticated." });
        }

        var sessions = await _attendanceService.GetManageableSessionsForTeacherAsync(userId);
        return Ok(new
        {
            success = true,
            data = sessions
        });
    }

    /// <summary>
    /// Get teacher attendance status for a specific session
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>Teacher attendance status or null if not checked in</returns>
    [HttpGet("teacher/status/{sessionId}")]
    public async Task<ActionResult> GetTeacherAttendanceStatus(int sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return Unauthorized(new { success = false, message = "User not authenticated." });
        }

        var status = await _attendanceService.GetTeacherAttendanceStatusAsync(userId, sessionId);

        if (status == null)
        {
            return Ok(new
            {
                success = true,
                message = "No attendance record found for this session.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            data = status
        });
    }
}
