using SmartClassRoom.Web.Models.ViewModels.Grading;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface IGradingService
{
    // Component Configuration
    Task<(bool Success, string Message)> SaveGradeComponentsAsync(int courseOfferingId, List<GradeComponentDto> components);
    Task<IEnumerable<GradeComponentDto>> GetGradeComponentsAsync(int courseOfferingId);

    // Individual Grade Entry
    Task<(bool Success, string Message)> SaveGradeAsync(SaveGradeDto gradeDto);
    Task<StudentGradeDto?> GetStudentGradesAsync(int studentId, int courseOfferingId);

    // Final Grade Calculation
    Task<(bool Success, string Message, FinalGradeCalculationResult? Result)> CalculateFinalGradesAsync(int courseOfferingId);

    // GPA Calculation
    Task<GPADto?> CalculateGPAAsync(int studentId, int? termId = null);
    Task UpdateStudentCumulativeGPAAsync(int studentId);

    // Grade Sheet (for UI)
    Task<GradeSheetDto> GetGradeSheetAsync(int courseOfferingId);

    // Template Management
    Task<(bool Success, string Message, int? TemplateId)> SaveTemplateAsync(SaveTemplateRequest request);
    Task<IEnumerable<GradeComponentTemplateDto>> GetTemplatesAsync(int? departmentId = null);
    Task<(bool Success, string Message)> ApplyTemplateAsync(int courseOfferingId, int templateId);
}
