using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

/// <summary>
/// Service for managing lookup data using Unit of Work
/// </summary>
public class LookupService : ILookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public LookupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
    {
        return await _unitOfWork.Departments.GetAllAsync();
    }

    public async Task<IEnumerable<Level>> GetAllLevelsAsync()
    {
        var levels = await _unitOfWork.Repository<Level>().GetAllAsync();
        return levels.OrderBy(l => l.Name);
    }

    public async Task<IEnumerable<Section>> GetAllSectionsAsync()
    {
        var sections = await _unitOfWork.Repository<Section>().GetAllAsync();
        return sections.OrderBy(s => s.Name);
    }

    public async Task<IEnumerable<Group>> GetAllGroupsAsync()
    {
        var groups = await _unitOfWork.Repository<Group>().GetAllAsync();
        return groups.OrderBy(g => g.Name);
    }

    public async Task<IEnumerable<University>> GetAllUniversitiesAsync()
    {
        var universities = await _unitOfWork.Repository<University>().GetAllAsync();
        return universities.OrderBy(u => u.Name);
    }

    public async Task<IEnumerable<Level>> GetLevelsByDepartmentAsync(int departmentId)
    {
        return await _unitOfWork.Levels.GetActiveLevelsByDepartmentAsync(departmentId);
    }

    public async Task<IEnumerable<Section>> GetSectionsByLevelAsync(int levelId)
    {
        return await _unitOfWork.Sections.GetActiveSectionsByLevelAsync(levelId);
    }
}
