namespace SmartClassRoom.Web.Models.DTOs.FaceRecognition;

/// <summary>
/// Response DTO for face verification
/// </summary>
public class VerifyFaceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public FaceVerificationData? Data { get; set; }
}

/// <summary>
/// Face verification data containing match result and similarity
/// </summary>
public class FaceVerificationData
{
    /// <summary>
    /// Whether the face was verified (matched stored embedding)
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Similarity percentage (0-100)
    /// </summary>
    public double Similarity { get; set; }
}

/// <summary>
/// Response DTO for face enrollment status check
/// </summary>
public class FaceStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public FaceStatusData? Data { get; set; }
}

/// <summary>
/// Face enrollment status data
/// </summary>
public class FaceStatusData
{
    /// <summary>
    /// Whether the user has enrolled their face
    /// </summary>
    public bool HasFaceEnrollment { get; set; }
}
