using Microsoft.AspNetCore.Identity;

namespace SmartClassRoom.Web.Models.Entities.Identity
{
    /// <summary>
    /// Custom role claim entity
    /// </summary>
    public class ApplicationRoleClaim : IdentityRoleClaim<int>
    {
        // Identity Framework provides:
        // - Id (int)
        // - RoleId (int)
        // - ClaimType (string)
        // - ClaimValue (string)
    }
}
