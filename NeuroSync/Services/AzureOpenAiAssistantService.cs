using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using NeuroSync.Api.DTOs;
using OpenAI.Chat;

namespace NeuroSync.Api.Services;

// Azure OpenAI implementation of the AI assistant. Used in production (more
// reliable + keeps data in-tenant than the public Gemini API). Selected in
// Program.cs when "AzureOpenAI:Endpoint" is configured.
public class AzureOpenAiAssistantService : IAiAssistantService
{
    private readonly ChatClient _chat;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AzureOpenAiAssistantService(IConfiguration config)
    {
        var endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint missing");
        var apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey missing");
        var deployment = config["AzureOpenAI:Deployment"] ?? "gpt-4o-mini";

        var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
        _chat = client.GetChatClient(deployment);
    }

    private async Task<string> CompleteJsonAsync(string prompt)
    {
        var options = new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() };
        var messages = new List<ChatMessage> { new UserChatMessage(prompt) };
        ClientResult<ChatCompletion> result = await _chat.CompleteChatAsync(messages, options);
        var content = result.Value.Content;
        return content.Count > 0 ? content[0].Text : string.Empty;
    }

    public async Task<AiTaskBreakdownResponse> BreakTaskIntoMicroStepsAsync(string rawTaskText, string personalization = "")
    {
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
                    ""description"": ""Simple, clear instructions with sufficient detail"",
                    ""estimatedMinutes"": 5
                }}
            ]
        }}

        Rules:
        - Break the task into 3 to 7 clear, ordered steps.
        - estimatedMinutes must be a whole NUMBER.
        - Use simple, scan-friendly language for neurodiverse users.{personalization}";

        var json = await CompleteJsonAsync(prompt);
        if (string.IsNullOrEmpty(json)) throw new Exception("AI returned an empty response.");
        return JsonSerializer.Deserialize<AiTaskBreakdownResponse>(json, JsonOpts) ?? new AiTaskBreakdownResponse();
    }

    public async Task<DocumentAnalysisResult> SummarizeDocumentAsync(string documentText, string personalization = "")
    {
        var prompt = $@"
    You are a Dyslexia-friendly Document Assistant.
    Analyze the following text: ""{documentText}""

    Respond ONLY in valid JSON format with the following fields:
    - summary: A 2-3 sentence overview using very simple words.
    - actionItems: A list of specific, explicit tasks found in the text.
    - deadline: Any date or time mentioned. If none, say 'No specific deadline'.
    - tone: Describe the mood (e.g., Urgent, Supportive, Professional, Demanding).
    - highlights: 3 to 5 very short key points (phrases, not full sentences).
    - hiddenTasks: Tasks that are only implied, not stated directly. If none, return an empty list.
    - simplifiedText: Rewrite the original content for someone with Dyslexia.
      Use:
      - Very short sentences.
      - Clear headings.
      - Bullet points for lists.
      - No complex vocabulary.

    JSON Structure:
    {{
        ""summary"": ""string"",
        ""actionItems"": [""string"", ""string""],
        ""deadline"": ""string"",
        ""tone"": ""string"",
        ""highlights"": [""string"", ""string""],
        ""hiddenTasks"": [""string""],
        ""simplifiedText"": ""string""
    }}{personalization}";

        var json = await CompleteJsonAsync(prompt);
        if (string.IsNullOrEmpty(json)) return new DocumentAnalysisResult { Summary = "Error: AI returned empty content." };
        return JsonSerializer.Deserialize<DocumentAnalysisResult>(json, JsonOpts) ?? new DocumentAnalysisResult();
    }

    public async Task<AiTaskBreakdownResponse> BuildTaskFromActionsAsync(string documentText, List<string> actionItems, string personalization = "")
    {
        var items = string.Join("\n", actionItems.Select((a, i) => $"{i + 1}. {a}"));

        var prompt = $@"
        You are an expert ADHD Productivity Coach.
        Original text the person needs to act on: ""{documentText}""

        These are the action items already identified — use them as the subtask names (keep them EXACTLY and in the same order):
        {items}

        Respond ONLY in JSON format with this structure:
        {{
            ""taskTitle"": ""Clear overall task title (max 6 words)"",
            ""taskSummary"": ""2-3 sentences in simple language summarising the overall task"",
            ""steps"": [
                {{
                    ""heading"": ""the exact action item text"",
                    ""description"": ""1-2 short, clear sentences explaining how to do this step, using the context of the original text"",
                    ""estimatedMinutes"": 15
                }}
            ]
        }}

        Rules:
        - Keep each step heading EXACTLY as the provided action item, in the same order. Do not add or remove steps.
        - estimatedMinutes must be a whole NUMBER.
        - Use simple, scan-friendly language for neurodiverse users.{personalization}";

        var json = await CompleteJsonAsync(prompt);
        if (string.IsNullOrEmpty(json)) throw new Exception("AI returned an empty response.");
        return JsonSerializer.Deserialize<AiTaskBreakdownResponse>(json, JsonOpts) ?? new AiTaskBreakdownResponse();
    }
}
