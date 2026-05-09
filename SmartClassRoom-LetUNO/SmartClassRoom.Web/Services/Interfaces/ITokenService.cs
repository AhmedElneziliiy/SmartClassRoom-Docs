using SmartClassRoom.Web.Models.Entities.Identity;
using System.Security.Claims;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
