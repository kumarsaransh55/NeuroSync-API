namespace NeuroSync.Api.DTOs;

public class QuickCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DueDate { get; set; }
    public int? ProjectId { get; set; }
}

public class TaskStepDto
{
    public string Heading { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public int ProgressPercentage { get; set; }
    public string? DueDate { get; set; }
    public int? ProjectId { get; set; }
    public List<TaskStepDto> Steps { get; set; } = new();
}
