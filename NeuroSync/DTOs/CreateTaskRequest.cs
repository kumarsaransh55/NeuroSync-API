namespace NeuroSync.Api.DTOs;

public class CreateTaskRequest
{
    public string UserId { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
}