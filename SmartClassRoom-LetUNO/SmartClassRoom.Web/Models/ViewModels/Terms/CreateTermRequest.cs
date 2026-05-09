using System.ComponentModel.DataAnnotations;

namespace SmartClassRoom.Web.Models.ViewModels.Terms;

public class CreateTermRequest
{
    [Required(ErrorMessage = "Term name is required")]
    [MaxLength(100, ErrorMessage = "Term name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    public string Status { get; set; } = "Upcoming";

    public bool IsActive { get; set; } = true;
}
