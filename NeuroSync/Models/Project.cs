using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.Models;

public class Project
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#166534";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // A project groups many tasks.
    public List<TaskItem> Tasks { get; set; } = new();
}
