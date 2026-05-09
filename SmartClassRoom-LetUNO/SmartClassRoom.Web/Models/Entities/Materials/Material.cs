using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Identity;

namespace SmartClassRoom.Web.Models.Entities.Materials
{
    /// <summary>
    /// Course material entity
    /// </summary>
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; }

        [MaxLength(50)]
        public string? FileType { get; set; } // PDF, DOCX, PPTX, etc.

        public long FileSize { get; set; } // in bytes

        public int CourseOfferingId { get; set; }
        public int? FolderId { get; set; }

        public int UploadedById { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int? DownloadCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual CourseOffering CourseOffering { get; set; }
        public virtual MaterialFolder? Folder { get; set; }

        [ForeignKey("UploadedById")]
        public virtual ApplicationUser UploadedBy { get; set; }
    }
}
