using System.ComponentModel.DataAnnotations;
using SmartClassRoom.Web.Models.Entities.Academic;

namespace SmartClassRoom.Web.Models.Entities.Materials
{
    /// <summary>
    /// Material folder/category entity
    /// </summary>
    public class MaterialFolder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int? ParentFolderId { get; set; } // For nested folders

        public int CourseOfferingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual MaterialFolder? ParentFolder { get; set; }
        public virtual CourseOffering CourseOffering { get; set; }

        // Collections
        public virtual ICollection<MaterialFolder> SubFolders { get; set; } = new List<MaterialFolder>();
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
    }
}
