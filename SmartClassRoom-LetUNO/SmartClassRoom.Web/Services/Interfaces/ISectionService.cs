using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Sections;

namespace SmartClassRoom.Web.Services.Interfaces;

/// <summary>
/// Service interface for managing Section entities
/// </summary>
public interface ISectionService
{
    /// <summary>
    /// Get paginated list of sections with optional filtering and sorting
    /// </summary>
    Task<PagedResult<Section>> GetSectionsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? levelId = null,
        int? departmentId = null,
        bool? isActive = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Get section by ID
    /// </summary>
    Task<Section?> GetSectionByIdAsync(int id);

    /// <summary>
    /// Get section with all related details loaded
    /// </summary>
    Task<Section?> GetSectionWithDetailsAsync(int id);

    /// <summary>
    /// Check if section code already exists
    /// </summary>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Create a new section
    /// </summary>
    Task<(bool Success, string Message)> CreateSectionAsync(CreateSectionRequest request);

    /// <summary>
    /// Update an existing section
    /// </summary>
    Task<(bool Success, string Message)> UpdateSectionAsync(EditSectionRequest request);

    /// <summary>
    /// Deactivate a section (soft delete)
    /// </summary>
    Task<(bool Success, string Message)> DeactivateSectionAsync(int id);

    /// <summary>
    /// Activate a section
    /// </summary>
    Task<(bool Success, string Message)> ActivateSectionAsync(int id);

    /// <summary>
    /// Get statistics for a specific section
    /// </summary>
    Task<SectionStatistics> GetSectionStatisticsAsync(int id);
}
