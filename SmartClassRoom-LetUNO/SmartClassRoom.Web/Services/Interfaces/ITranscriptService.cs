using SmartClassRoom.Web.Models.ViewModels.Grading;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface ITranscriptService
{
    Task<TranscriptDto?> GenerateTranscriptDataAsync(int studentId);
    Task<byte[]> GenerateTranscriptPdfAsync(int studentId);
}
