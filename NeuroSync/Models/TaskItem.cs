using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.Models;

public class TaskItem
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string OriginalSourceText { get; set; } = string.Empty;
    public DateTime? SuggestedDeadline { get; set; }
    public bool IsInFocusMode { get; set; } = false;
    public int ProgressPercentage { get; set; } = 0;

    // Relationships
    public List<TaskStep> MicroSteps { get; set; } = new();
    public List<Reminder> Reminders { get; set; } = new();
}