using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.Models;

public class Reminder
{
    [Key]
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; } // Navigation property
    public DateTime ScheduledTime { get; set; }
    public string Type { get; set; } = "Soft"; // Soft, Email, Calendar
    public int EscalationLevel { get; set; } = 0; // 0: Gentle, 1: Repeated, 2: Escalated
    public bool IsAcknowledged { get; set; } = false;
}