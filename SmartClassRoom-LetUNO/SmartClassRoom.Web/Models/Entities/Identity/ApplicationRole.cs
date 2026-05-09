using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.Entities.Identity
{
    /// <summary>
    /// Custom role entity for granular permissions
    /// </summary>
    public class ApplicationRole : IdentityRole<int>
    {
        // Identity Framework provides:
        // - Id (int)
        // - Name
        // - NormalizedName
        // - ConcurrencyStamp

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();
    }
}
