using Microsoft.AspNetCore.Identity;

namespace SmartClassRoom.Web.Models.Entities.Identity
{
    /// <summary>
    /// Junction table for User-Role relationship
    /// </summary>
    public class ApplicationUserRole : IdentityUserRole<int>
    {
        // Identity Framework provides:
        // - UserId (int)
        // - RoleId (int)

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}
