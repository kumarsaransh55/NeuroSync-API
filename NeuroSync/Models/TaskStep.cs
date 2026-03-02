using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.Models;

public class TaskStep
{
    [Key]
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
}