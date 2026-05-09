using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Entities.Materials;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Material entity
/// </summary>
public class MaterialRepository : Repository<Material>, IMaterialRepository
{
    public MaterialRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Material>> GetByCourseOfferingAsync(int courseOfferingId)
    {
        return await _dbSet
            .Where(m => m.CourseOfferingId == courseOfferingId && m.IsActive)
            .Include(m => m.Folder)
            .Include(m => m.UploadedBy)
            .OrderBy(m => m.FolderId)
            .ThenByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Material>> GetByFolderAsync(int folderId)
    {
        return await _dbSet
            .Where(m => m.FolderId == folderId && m.IsActive)
            .Include(m => m.UploadedBy)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Material>> GetRootMaterialsAsync(int courseOfferingId)
    {
        return await _dbSet
            .Where(m => m.CourseOfferingId == courseOfferingId && m.FolderId == null && m.IsActive)
            .Include(m => m.UploadedBy)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<Material?> GetWithDetailsAsync(int materialId)
    {
        return await _dbSet
            .Include(m => m.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(m => m.CourseOffering)
                .ThenInclude(co => co.Teacher)
            .Include(m => m.Folder)
            .Include(m => m.UploadedBy)
            .FirstOrDefaultAsync(m => m.Id == materialId);
    }

    public async Task<int> GetCountByCourseOfferingAsync(int courseOfferingId)
    {
        return await _dbSet
            .CountAsync(m => m.CourseOfferingId == courseOfferingId && m.IsActive);
    }

    public async Task<IEnumerable<Material>> GetByFileTypeAsync(int courseOfferingId, string fileType)
    {
        return await _dbSet
            .Where(m => m.CourseOfferingId == courseOfferingId &&
                       m.FileType == fileType &&
                       m.IsActive)
            .Include(m => m.UploadedBy)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Material>> GetByUploaderAsync(int teacherId)
    {
        return await _dbSet
            .Where(m => m.UploadedById == teacherId && m.IsActive)
            .Include(m => m.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(m => m.Folder)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task IncrementDownloadCountAsync(int materialId)
    {
        var material = await _dbSet.FindAsync(materialId);
        if (material != null)
        {
            material.DownloadCount = (material.DownloadCount ?? 0) + 1;
            _context.Entry(material).State = EntityState.Modified;
        }
    }

    public async Task<bool> ExistsAndActiveAsync(int materialId)
    {
        return await _dbSet
            .AnyAsync(m => m.Id == materialId && m.IsActive);
    }
}
