using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.Models;

public class TaskStep
{
    [Key]
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public string Heading { get; set; } = string.Empty; // NEW: AI Heading
    public string Description { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
}