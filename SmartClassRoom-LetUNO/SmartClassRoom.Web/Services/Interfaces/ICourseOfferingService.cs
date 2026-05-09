using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.ViewModels.Offerings;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface ICourseOfferingService
{
    Task<PagedResult<CourseOffering>> GetOfferingsPagedAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? termId = null,
        int? courseId = null,
        int? teacherId = null,
        int? sectionId = null,
        string? status = null,
        string? sortBy = null,
        bool ascending = true);

    Task<CourseOffering?> GetOfferingByIdAsync(int id);
    Task<CourseOffering?> GetOfferingWithDetailsAsync(int id);
    Task<OfferingStatistics> GetOfferingStatisticsAsync(int id);
    Task<IEnumerable<CourseOffering>> GetOfferingsByTermAsync(int termId);
    Task<IEnumerable<CourseOffering>> GetOfferingsByTeacherAsync(int teacherId);
    Task<(bool Success, string Message, int? OfferingId)> CreateOfferingAsync(CreateOfferingRequest request);
    Task<(bool Success, string Message)> UpdateOfferingAsync(EditOfferingRequest request);
    Task<(bool Success, string Message)> CancelOfferingAsync(int id);
}
