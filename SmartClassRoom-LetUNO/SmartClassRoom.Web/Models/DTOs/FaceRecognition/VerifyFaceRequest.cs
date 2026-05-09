using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.DTOs.FaceRecognition;

/// <summary>
/// Request DTO for face verification
/// </summary>
public class VerifyFaceRequest
{
    /// <summary>
    /// Base64 encoded face image (can include data URI prefix)
    /// </summary>
    [Required(ErrorMessage = "FaceImage is required")]
    public string FaceImage { get; set; } = null!;
}
