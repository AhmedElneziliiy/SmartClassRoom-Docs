using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.DTOs.Request;

public class ResetPasswordRequest
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
