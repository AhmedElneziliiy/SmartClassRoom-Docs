using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Universities;

/// <summary>
/// Data Transfer Object for creating a new University
/// </summary>
public class CreateUniversityRequest
{
    [Required(ErrorMessage = "University Name is required")]
    [MaxLength(200, ErrorMessage = "University Name cannot exceed 200 characters")]
    [Display(Name = "University Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "University Code cannot exceed 50 characters")]
    [Display(Name = "University Code")]
    public string? Code { get; set; }

    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "Website URL cannot exceed 200 characters")]
    [Url(ErrorMessage = "Invalid website URL format")]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;
}
