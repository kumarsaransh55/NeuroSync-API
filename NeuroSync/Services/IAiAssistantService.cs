using NeuroSync.Api.DTOs;

namespace NeuroSync.Api.Services;

public interface IAiAssistantService
{
    Task<AiTaskBreakdownResponse> BreakTaskIntoMicroStepsAsync(string rawTaskText);
}