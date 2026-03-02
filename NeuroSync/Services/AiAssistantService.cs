using NeuroSync.Api.DTOs;

namespace NeuroSync.Api.Services;

public class AiAssistantService : IAiAssistantService
{
    public async Task<List<string>> BreakTaskIntoMicroStepsAsync(string rawTaskText)
    {
        // TODO: Call OpenAI API here.
        // Simulated AI Response:
        await Task.Delay(1000);
        return new List<string> {
            "Open the document.",
            "Read the first paragraph.",
            "Write down 3 main points."
        };
    }

    public async Task<DocumentAnalysisResult> SummarizeDocumentAsync(string documentText)
    {
        // TODO: Call OpenAI API here.
        await Task.Delay(1000);
        return new DocumentAnalysisResult
        {
            Summary = "This is a simplified summary.",
            Tone = "Neutral",
            ActionItems = new List<string> { "Reply to email" },
            SimplifiedText = "Keep it simple. Reply to the email today."
        };
    }
}