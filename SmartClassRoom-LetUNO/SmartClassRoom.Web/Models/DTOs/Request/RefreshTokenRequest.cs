using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.DTOs.Request;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Access token is required")]
    public string AccessToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}
