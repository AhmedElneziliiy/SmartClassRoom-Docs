using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data;

namespace SmartClassRoom.Web.Controllers.API;

[Route("api/academic")]
[ApiController]
public class AcademicApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AcademicApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("levels/{departmentId}")]
    public async Task<IActionResult> GetLevelsByDepartment(int departmentId)
    {
        var levels = await _context.Levels
            .Where(l => l.DepartmentId == departmentId && l.IsActive)
            .OrderBy(l => l.LevelNumber)
            .Select(l => new { value = l.Id, text = l.Name })
            .ToListAsync();

        return Ok(levels);
    }

    [HttpGet("sections/{levelId}")]
    public async Task<IActionResult> GetSectionsByLevel(int levelId)
    {
        var sections = await _context.Sections
            .Where(s => s.LevelId == levelId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { value = s.Id, text = s.Name })
            .ToListAsync();

        return Ok(sections);
    }

    [HttpGet("groups/{sectionId}")]
    public async Task<IActionResult> GetGroupsBySection(int sectionId)
    {
        var groups = await _context.Groups
            .Where(g => g.SectionId == sectionId && g.IsActive)
            .OrderBy(g => g.Name)
            .Select(g => new { value = g.Id, text = g.Name })
            .ToListAsync();

        return Ok(groups);
    }
}
