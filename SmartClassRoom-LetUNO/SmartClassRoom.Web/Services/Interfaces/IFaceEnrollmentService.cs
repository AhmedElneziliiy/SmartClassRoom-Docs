namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service for face enrollment and verification operations
/// </summary>
public interface IFaceEnrollmentService
{
    /// <summary>
    /// Enroll a user's face by extracting embedding from photo
    /// </summary>
    /// <param name="userId">User ID to enroll face for</param>
    /// <param name="imageBytes">Image bytes (JPG, PNG)</param>
    /// <returns>Enrollment result with success status and message</returns>
    Task<FaceEnrollmentResult> EnrollFaceAsync(int userId, byte[] imageBytes);

    /// <summary>
    /// Verify a face image against a user's stored embedding
    /// </summary>
    /// <param name="userId">User ID to verify against</param>
    /// <param name="imageBytes">Image bytes to verify</param>
    /// <returns>Verification result with similarity score</returns>
    Task<FaceVerificationResult> VerifyFaceAsync(int userId, byte[] imageBytes);

    /// <summary>
    /// Check if user has face enrolled
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user has valid face enrollment</returns>
    Task<bool> HasFaceEnrollmentAsync(int userId);

    /// <summary>
    /// Delete user's face enrollment
    /// </summary>
    /// <param name="userId">User ID to delete enrollment for</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteFaceEnrollmentAsync(int userId);
}

/// <summary>
/// Result of face enrollment operation
/// </summary>
public class FaceEnrollmentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? FaceCount { get; set; }
}

/// <summary>
/// Result of face verification operation
/// </summary>
public class FaceVerificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public double Similarity { get; set; }
}
