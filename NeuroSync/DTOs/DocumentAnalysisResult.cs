namespace NeuroSync.Api.DTOs;

public class DocumentAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public List<string> ActionItems { get; set; } = new();
    public string Deadline { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string SimplifiedText { get; set; } = string.Empty;

    // Extra fields that power the frontend "Key Highlights" and "Hidden Tasks" cards.
    public List<string> Highlights { get; set; } = new();
    public List<string> HiddenTasks { get; set; } = new();
}