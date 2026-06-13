using NeuroSync.Api.DTOs;

namespace NeuroSync.Api.Services;

public interface IAiAssistantService
{
    Task<AiTaskBreakdownResponse> BreakTaskIntoMicroStepsAsync(string rawTaskText, string personalization = "");
    Task<DocumentAnalysisResult> SummarizeDocumentAsync(string documentText, string personalization = "");
    Task<AiTaskBreakdownResponse> BuildTaskFromActionsAsync(string documentText, List<string> actionItems, string personalization = "");
}