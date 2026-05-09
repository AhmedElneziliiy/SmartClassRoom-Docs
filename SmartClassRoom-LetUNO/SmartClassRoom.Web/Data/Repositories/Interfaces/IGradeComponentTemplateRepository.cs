using SmartClassRoom.Web.Models.Entities.Grading;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IGradeComponentTemplateRepository : IRepository<GradeComponentTemplate>
{
    Task<IEnumerable<GradeComponentTemplate>> GetActiveTemplatesAsync(int? departmentId = null);
    Task<GradeComponentTemplate?> GetTemplateWithItemsAsync(int templateId);
    Task<bool> TemplateNameExistsAsync(string templateName, int? departmentId, int? excludeId = null);
}
