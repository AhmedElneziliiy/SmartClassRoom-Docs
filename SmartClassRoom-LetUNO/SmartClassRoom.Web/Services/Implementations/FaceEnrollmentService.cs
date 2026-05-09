using FaceRecognition.Core.Services;
using Microsoft.AspNetCore.Identity;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service for face enrollment and verification using FaceRecognition.Core
/// </summary>
public class FaceEnrollmentService : IFaceEnrollmentService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFaceVerificationService _faceService;
    private readonly ILogger<FaceEnrollmentService> _logger;

    public FaceEnrollmentService(
        UserManager<ApplicationUser> userManager,
        IFaceVerificationService faceService,
        ILogger<FaceEnrollmentService> logger)
    {
        _userManager = userManager;
        _faceService = faceService;
        _logger = logger;
    }

    public async Task<FaceEnrollmentResult> EnrollFaceAsync(int userId, byte[] imageBytes)
    {
        try
        {
            // Validate image size (max 5MB)
            if (imageBytes.Length > 5 * 1024 * 1024)
            {
                return new FaceEnrollmentResult
                {
                    Success = false,
                    Message = "Image size exceeds 5MB limit"
                };
            }

            // Extract face embedding
            var extractResult = await _faceService.ExtractEmbeddingAsync(imageBytes);

            if (!extractResult.Success)
            {
                return new FaceEnrollmentResult
                {
                    Success = false,
                    Message = extractResult.ErrorMessage ?? "Failed to extract face embedding",
                    FaceCount = extractResult.FaceCount
                };
            }

            // Validate exactly one face
            if (extractResult.FaceCount == 0)
            {
                return new FaceEnrollmentResult
                {
                    Success = false,
                    Message = "No face detected in the image. Please upload a clear photo with your face visible.",
                    FaceCount = 0
                };
            }

            if (extractResult.FaceCount > 1)
            {
                return new FaceEnrollmentResult
                {
                    Success = false,
                    Message = $"Multiple faces ({extractResult.FaceCount}) detected. Please upload a photo with only your face.",
                    FaceCount = extractResult.FaceCount
                };
            }

            // Convert float[] embedding to Base64 string
            var embeddingBytes = new byte[extractResult.Embedding!.Length * sizeof(float)];
            Buffer.BlockCopy(extractResult.Embedding, 0, embeddingBytes, 0, embeddingBytes.Length);
            var embeddingBase64 = Convert.ToBase64String(embeddingBytes);

            _logger.LogInformation(
                "Enrollment embedding for user {UserId}: Length={Length}, First5=[{V0:F4},{V1:F4},{V2:F4},{V3:F4},{V4:F4}]",
                userId,
                extractResult.Embedding.Length,
                extractResult.Embedding[0],
                extractResult.Embedding[1],
                extractResult.Embedding[2],
                extractResult.Embedding[3],
                extractResult.Embedding[4]);

            // Update user record
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new FaceEnrollmentResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            user.FaceEmbeddingBase64 = embeddingBase64;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new FaceEnrollmentResult
                {
                    Success = false,
                    Message = "Failed to save face embedding to database"
                };
            }

            _logger.LogInformation("Face enrolled successfully for user {UserId}", userId);

            return new FaceEnrollmentResult
            {
                Success = true,
                Message = "Face enrolled successfully",
                FaceCount = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling face for user {UserId}", userId);
            return new FaceEnrollmentResult
            {
                Success = false,
                Message = $"Error enrolling face: {ex.Message}"
            };
        }
    }

    public async Task<FaceVerificationResult> VerifyFaceAsync(int userId, byte[] imageBytes)
    {
        try
        {
            // Get user with stored embedding
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new FaceVerificationResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if user has face enrolled
            if (string.IsNullOrEmpty(user.FaceEmbeddingBase64) ||
                user.FaceEmbeddingBase64 == "PLACEHOLDER_EMBEDDING")
            {
                return new FaceVerificationResult
                {
                    Success = false,
                    Message = "User has no face enrolled. Please contact admin to enroll your face."
                };
            }

            // Extract embedding from submitted image
            var extractResult = await _faceService.ExtractEmbeddingAsync(imageBytes);

            if (!extractResult.Success)
            {
                return new FaceVerificationResult
                {
                    Success = false,
                    Message = extractResult.ErrorMessage ?? "Failed to detect face in image"
                };
            }

            if (extractResult.FaceCount == 0)
            {
                return new FaceVerificationResult
                {
                    Success = false,
                    Message = "No face detected in the image"
                };
            }

            if (extractResult.FaceCount > 1)
            {
                return new FaceVerificationResult
                {
                    Success = false,
                    Message = "Multiple faces detected. Please submit image with only your face."
                };
            }

            // Convert stored Base64 back to float[]
            var storedEmbeddingBytes = Convert.FromBase64String(user.FaceEmbeddingBase64);
            var storedEmbedding = new float[storedEmbeddingBytes.Length / sizeof(float)];
            Buffer.BlockCopy(storedEmbeddingBytes, 0, storedEmbedding, 0, storedEmbeddingBytes.Length);

            _logger.LogInformation(
                "Verification - New embedding: Length={Length}, First5=[{V0:F4},{V1:F4},{V2:F4},{V3:F4},{V4:F4}]",
                extractResult.Embedding!.Length,
                extractResult.Embedding[0],
                extractResult.Embedding[1],
                extractResult.Embedding[2],
                extractResult.Embedding[3],
                extractResult.Embedding[4]);

            _logger.LogInformation(
                "Verification - Stored embedding: Length={Length}, First5=[{V0:F4},{V1:F4},{V2:F4},{V3:F4},{V4:F4}]",
                storedEmbedding.Length,
                storedEmbedding[0],
                storedEmbedding[1],
                storedEmbedding[2],
                storedEmbedding[3],
                storedEmbedding[4]);

            // Compare embeddings
            var similarityResult = _faceService.CompareSimilarity(extractResult.Embedding!, storedEmbedding);

            _logger.LogInformation(
                "Face verification for user {UserId}: Similarity={Similarity}%, IsMatch={IsMatch}",
                userId, similarityResult.Similarity, similarityResult.IsMatch);

            return new FaceVerificationResult
            {
                Success = true,
                Message = similarityResult.IsMatch ? "Face verified successfully" : "Face does not match",
                IsVerified = similarityResult.IsMatch,
                Similarity = similarityResult.Similarity
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying face for user {UserId}", userId);
            return new FaceVerificationResult
            {
                Success = false,
                Message = $"Error verifying face: {ex.Message}"
            };
        }
    }

    public async Task<bool> HasFaceEnrollmentAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null &&
               !string.IsNullOrEmpty(user.FaceEmbeddingBase64) &&
               user.FaceEmbeddingBase64 != "PLACEHOLDER_EMBEDDING";
    }

    public async Task<bool> DeleteFaceEnrollmentAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.FaceEmbeddingBase64 = null;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
