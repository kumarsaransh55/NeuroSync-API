using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.Models;

public class UserSettings
{
    [Key]
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty; // In a real app, links to Auth User
    public bool UseDyslexiaFont { get; set; } = false;
    public string Theme { get; set; } = "Light";
    public string NotificationSensitivity { get; set; } = "Normal";
    public bool VisualReminders { get; set; } = true;

    // The full settings + personalization profile as JSON (flexible, no schema churn).
    public string PreferencesJson { get; set; } = "{}";
}