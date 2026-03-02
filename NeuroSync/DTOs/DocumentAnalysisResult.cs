namespace NeuroSync.Api.DTOs;

public class DocumentAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public List<string> ActionItems { get; set; } = new();
    public string Deadline { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string SimplifiedText { get; set; } = string.Empty;
}