using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Materials;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for MaterialFolder entity
/// </summary>
public class MaterialFolderRepository : Repository<MaterialFolder>, IMaterialFolderRepository
{
    public MaterialFolderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MaterialFolder>> GetByCourseOfferingAsync(int courseOfferingId)
    {
        return await _dbSet
            .Where(f => f.CourseOfferingId == courseOfferingId)
            .Include(f => f.Materials.Where(m => m.IsActive))
            .Include(f => f.SubFolders)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MaterialFolder>> GetRootFoldersAsync(int courseOfferingId)
    {
        return await _dbSet
            .Where(f => f.CourseOfferingId == courseOfferingId && f.ParentFolderId == null)
            .Include(f => f.Materials.Where(m => m.IsActive))
            .Include(f => f.SubFolders)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MaterialFolder>> GetSubFoldersAsync(int parentFolderId)
    {
        return await _dbSet
            .Where(f => f.ParentFolderId == parentFolderId)
            .Include(f => f.Materials.Where(m => m.IsActive))
            .Include(f => f.SubFolders)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<MaterialFolder?> GetWithMaterialsAsync(int folderId)
    {
        return await _dbSet
            .Include(f => f.Materials.Where(m => m.IsActive))
                .ThenInclude(m => m.UploadedBy)
            .Include(f => f.SubFolders)
            .FirstOrDefaultAsync(f => f.Id == folderId);
    }

    public async Task<MaterialFolder?> GetWithNestedContentAsync(int folderId)
    {
        // Get the folder with first level of nesting
        var folder = await _dbSet
            .Include(f => f.Materials.Where(m => m.IsActive))
                .ThenInclude(m => m.UploadedBy)
            .Include(f => f.SubFolders)
                .ThenInclude(sf => sf.Materials.Where(m => m.IsActive))
            .Include(f => f.SubFolders)
                .ThenInclude(sf => sf.SubFolders)
            .FirstOrDefaultAsync(f => f.Id == folderId);

        return folder;
    }

    public async Task<bool> HasContentAsync(int folderId)
    {
        var hasMaterials = await _context.Set<Material>()
            .AnyAsync(m => m.FolderId == folderId && m.IsActive);

        if (hasMaterials) return true;

        var hasSubfolders = await _dbSet
            .AnyAsync(f => f.ParentFolderId == folderId);

        return hasSubfolders;
    }

    public async Task<int> GetMaterialCountAsync(int folderId, bool includeSubfolders = false)
    {
        if (!includeSubfolders)
        {
            return await _context.Set<Material>()
                .CountAsync(m => m.FolderId == folderId && m.IsActive);
        }

        // Get count including all nested subfolders (recursive)
        var folderIds = new List<int> { folderId };
        await GetAllSubfolderIdsAsync(folderId, folderIds);

        return await _context.Set<Material>()
            .CountAsync(m => m.FolderId.HasValue && folderIds.Contains(m.FolderId.Value) && m.IsActive);
    }

    private async Task GetAllSubfolderIdsAsync(int parentId, List<int> ids)
    {
        var subfolderIds = await _dbSet
            .Where(f => f.ParentFolderId == parentId)
            .Select(f => f.Id)
            .ToListAsync();

        foreach (var id in subfolderIds)
        {
            ids.Add(id);
            await GetAllSubfolderIdsAsync(id, ids);
        }
    }

    public async Task<bool> NameExistsAsync(int courseOfferingId, string name, int? parentFolderId, int? excludeFolderId = null)
    {
        var query = _dbSet
            .Where(f => f.CourseOfferingId == courseOfferingId &&
                       f.Name.ToLower() == name.ToLower() &&
                       f.ParentFolderId == parentFolderId);

        if (excludeFolderId.HasValue)
        {
            query = query.Where(f => f.Id != excludeFolderId.Value);
        }

        return await query.AnyAsync();
    }
}
