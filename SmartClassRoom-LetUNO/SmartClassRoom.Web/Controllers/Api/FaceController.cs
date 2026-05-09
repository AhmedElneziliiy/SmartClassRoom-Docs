using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Models.DTOs.FaceRecognition;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Controllers.Api;

/// <summary>
/// Face Recognition API - Isolated face verification endpoints
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FaceController : ControllerBase
{
    private readonly IFaceEnrollmentService _faceService;
    private readonly ILogger<FaceController> _logger;

    public FaceController(
        IFaceEnrollmentService faceService,
        ILogger<FaceController> logger)
    {
        _faceService = faceService;
        _logger = logger;
    }

    /// <summary>
    /// Verify user's face against stored enrollment
    /// </summary>
    /// <param name="request">Base64 encoded face image</param>
    /// <returns>Verification result with similarity score</returns>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyFaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<VerifyFaceResponse>> VerifyFace([FromBody] VerifyFaceRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new VerifyFaceResponse
            {
                Success = false,
                Message = "Invalid user token"
            });
        }

        // Validate and decode base64 image
        byte[] imageBytes;
        try
        {
            // Handle data URI format if present (e.g., "data:image/jpeg;base64,...")
            var base64Data = request.FaceImage;
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(',')[1];
            }
            imageBytes = Convert.FromBase64String(base64Data);
        }
        catch (FormatException)
        {
            return BadRequest(new VerifyFaceResponse
            {
                Success = false,
                Message = "Invalid base64 image format"
            });
        }

        var result = await _faceService.VerifyFaceAsync(userId, imageBytes);

        return Ok(new VerifyFaceResponse
        {
            Success = result.Success,
            Message = result.Message,
            Data = result.Success ? new FaceVerificationData
            {
                IsVerified = result.IsVerified,
                Similarity = result.Similarity
            } : null
        });
    }

    /// <summary>
    /// Check if current user has face enrolled
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(FaceStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FaceStatusResponse>> GetFaceStatus()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new FaceStatusResponse
            {
                Success = false,
                Message = "Invalid user token"
            });
        }

        var hasEnrollment = await _faceService.HasFaceEnrollmentAsync(userId);

        return Ok(new FaceStatusResponse
        {
            Success = true,
            Message = hasEnrollment ? "Face is enrolled" : "Face is not enrolled",
            Data = new FaceStatusData
            {
                HasFaceEnrollment = hasEnrollment
            }
        });
    }

    /// <summary>
    /// Health check for face recognition service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "FaceRecognition" });
    }
}
