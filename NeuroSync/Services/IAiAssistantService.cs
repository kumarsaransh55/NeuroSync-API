using NeuroSync.Api.DTOs;

namespace NeuroSync.Api.Services;

public interface IAiAssistantService
{
    Task<List<string>> BreakTaskIntoMicroStepsAsync(string rawTaskText);
    Task<DocumentAnalysisResult> SummarizeDocumentAsync(string documentText);
}