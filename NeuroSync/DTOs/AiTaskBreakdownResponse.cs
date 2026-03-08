namespace NeuroSync.Api.DTOs;

public class AiTaskBreakdownResponse
{
    public string TaskTitle { get; set; } = string.Empty; // AI Generated Title
    public string TaskSummary { get; set; } = string.Empty;
    public List<AiStepDto> Steps { get; set; } = new();
}

public class AiStepDto
{
    public string Heading { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
}