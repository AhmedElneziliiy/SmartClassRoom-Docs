using SmartClassRoom.Web.Models.Entities.Grading;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

public interface IGradeComponentRepository : IRepository<GradeComponent>
{
    // Get all components for a course offering
    Task<IEnumerable<GradeComponent>> GetComponentsByOfferingAsync(int courseOfferingId);

    // Get component with grades included
    Task<GradeComponent?> GetComponentWithGradesAsync(int componentId);

    // Validate that weights sum to 100%
    Task<decimal> GetTotalWeightAsync(int courseOfferingId);

    // Delete all components for an offering
    Task DeleteComponentsByOfferingAsync(int courseOfferingId);

    // Get component count for offering
    Task<int> GetComponentCountAsync(int courseOfferingId);
}
