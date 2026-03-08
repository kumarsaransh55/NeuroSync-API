using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using NeuroSync.Api.DTOs;

namespace NeuroSync.Api.Services;

public class AiAssistantService : IAiAssistantService
{
    private readonly string _apiKey;
    private readonly string _modelName = "gemini-3-flash-preview"; // Or gemini-2.0-flash-exp

    public AiAssistantService(IConfiguration config)
    {
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini API Key missing");
    }

    public async Task<AiTaskBreakdownResponse> BreakTaskIntoMicroStepsAsync(string rawTaskText)
    {
        // 1. Initialize the Google AI Client
        var client = new Client(apiKey: _apiKey);

        // 2. Craft the ADHD-focused prompt 
        var prompt = $@"
        You are an expert ADHD Productivity Coach. 
        Take this input: ""{rawTaskText}""

        Respond ONLY in JSON format with this structure:
        {{
            ""taskTitle"": ""Clear title (max 6 words) "",
            ""taskSummary"": ""2-4 sentences in simple language that summarises what needs to be done"",
            ""steps"": [
                {{
                    ""heading"": ""Action heading"",
                    ""description"": ""Simple clear instructions sufficient detail"",
                    ""estimatedMinutes"": 5
                }}
            ]
        }}
        
        Rules:
        - estimatedMinutes must be a whole NUMBER.
        - Use simple, scan-friendly language for neurodiverse users.";

        // 3. Configure the AI to return JSON
        var config = new GenerateContentConfig
        {
            ResponseMimeType = "application/json",
            Temperature = 0.7f
        };

        // 4. Call the SDK
        var response = await client.Models.GenerateContentAsync(
            model: _modelName,
            contents: prompt,
            config: config
        );

        // 5. Extract the text response
        var jsonText = response.Candidates[0].Content.Parts[0].Text;

        if (string.IsNullOrEmpty(jsonText))
        {
            throw new Exception("AI returned an empty response.");
        }

        // 6. Deserialize into our DTO
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<AiTaskBreakdownResponse>(jsonText, options)
               ?? new AiTaskBreakdownResponse();
    }

    // You can implement the SummarizeDocument method similarly using the SDK
    public async Task<DocumentAnalysisResult> SummarizeDocumentAsync(string documentText)
    {
        var client = new Client( apiKey: _apiKey);
        var prompt = $"Summarize this for a person with Dyslexia, use bullet points: {documentText}";

        var response = await client.Models.GenerateContentAsync(_modelName, prompt);

        return new DocumentAnalysisResult
        {
            Summary = response.Candidates[0].Content.Parts[0].Text
        };
    }
}