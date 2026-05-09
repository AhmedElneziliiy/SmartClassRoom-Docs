using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Groups;

public class CreateGroupRequest
{
    [Required(ErrorMessage = "Group Name is required")]
    [MaxLength(100, ErrorMessage = "Group Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
    public string? Code { get; set; }

    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
    public int? Capacity { get; set; }

    [Required(ErrorMessage = "Section is required")]
    public int SectionId { get; set; }

    public bool IsActive { get; set; } = true;
}
