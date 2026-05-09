using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Models.Entities.Identity;

/// <summary>
/// Custom user entity extending IdentityUser for Identity Framework
/// Base for all user types (Students, Teachers, Admins)
/// </summary>
public class ApplicationUser : IdentityUser<int> // Using int as primary key
{
    // Identity Framework provides:
    // - Id (int)
    // - UserName
    // - Email
    // - EmailConfirmed
    // - PasswordHash
    // - SecurityStamp
    // - PhoneNumber
    // - TwoFactorEnabled
    // - LockoutEnd
    // - LockoutEnabled
    // - AccessFailedCount

    // Custom Properties
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? NationalId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; } // Male/Female

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(255)]
    public string? ProfilePhotoPath { get; set; }

    // Face Recognition - Store Base64 Embedding
    public string? FaceEmbeddingBase64 { get; set; } // For your custom AI model

    // Mobile Device Authentication - Unique Device Identifier
    [MaxLength(100)]
    public string? UDID { get; set; } // For mobile device authentication

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int? UniversityId { get; set; }

    // Navigation Properties
    public virtual University? University { get; set; }

    // Discriminator for inheritance (TPH - Table Per Hierarchy)
    [MaxLength(50)]
    public string UserType { get; set; } = "User"; // "Student", "Teacher", "Admin"
}
