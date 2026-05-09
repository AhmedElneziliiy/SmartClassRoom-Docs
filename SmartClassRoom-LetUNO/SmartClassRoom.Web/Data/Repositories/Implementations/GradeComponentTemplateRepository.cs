using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Grading;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

public class GradeComponentTemplateRepository : Repository<GradeComponentTemplate>, IGradeComponentTemplateRepository
{
    public GradeComponentTemplateRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<GradeComponentTemplate>> GetActiveTemplatesAsync(int? departmentId = null)
    {
        var query = _context.GradeComponentTemplates
            .Include(t => t.Items)
            .Where(t => t.IsActive);

        if (departmentId.HasValue)
        {
            query = query.Where(t => t.DepartmentId == departmentId || t.DepartmentId == null);
        }

        return await query
            .OrderBy(t => t.TemplateName)
            .ToListAsync();
    }

    public async Task<GradeComponentTemplate?> GetTemplateWithItemsAsync(int templateId)
    {
        return await _context.GradeComponentTemplates
            .Include(t => t.Items.OrderBy(i => i.OrderIndex))
            .Include(t => t.Department)
            .FirstOrDefaultAsync(t => t.Id == templateId);
    }

    public async Task<bool> TemplateNameExistsAsync(string templateName, int? departmentId, int? excludeId = null)
    {
        var query = _context.GradeComponentTemplates
            .Where(t => t.TemplateName == templateName && t.IsActive);

        if (departmentId.HasValue)
        {
            query = query.Where(t => t.DepartmentId == departmentId);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId);
        }

        return await query.AnyAsync();
    }
}
