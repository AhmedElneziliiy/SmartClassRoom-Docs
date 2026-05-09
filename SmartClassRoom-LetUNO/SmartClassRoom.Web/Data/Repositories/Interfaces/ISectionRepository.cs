using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Section entity with domain-specific queries
/// </summary>
public interface ISectionRepository : IRepository<Section>
{
    /// <summary>
    /// Get all sections for a specific level
    /// </summary>
    Task<IEnumerable<Section>> GetByLevelAsync(int levelId);

    /// <summary>
    /// Get section by code
    /// </summary>
    Task<Section?> GetByCodeAsync(string code);

    /// <summary>
    /// Get all active sections for a specific level
    /// </summary>
    Task<IEnumerable<Section>> GetActiveSectionsByLevelAsync(int levelId);

    /// <summary>
    /// Get section with all related details loaded
    /// </summary>
    Task<Section?> GetSectionWithDetailsAsync(int sectionId);

    /// <summary>
    /// Get statistics for a specific section
    /// </summary>
    Task<SectionStatistics> GetSectionStatisticsAsync(int sectionId);
}
