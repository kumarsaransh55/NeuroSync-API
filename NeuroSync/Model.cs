using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NeuroSync
{
    public class Model
    {
    }
    // 1. User & Accessibility Settings
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public UserSettings Settings { get; set; }
    }

    public class UserSettings
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }

        // Accessibility & Personalization
        public bool UseDyslexiaFont { get; set; } = false;
        public string Theme { get; set; } = "Light"; // Light, Dark, Minimal, Noise-Free
        public string NotificationSensitivity { get; set; } = "Normal"; // Low, Normal, Adaptive
        public bool VisualReminders { get; set; } = true;
    }

    // 2. Smart Task Planner
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string OriginalSourceText { get; set; } // e.g., pasted email
        public DateTime? SuggestedDeadline { get; set; }
        public bool IsInFocusMode { get; set; } = false;
        public int ProgressPercentage { get; set; } = 0;

        public List<TaskStep> MicroSteps { get; set; } = new();
        public List<Reminder> Reminders { get; set; } = new();
    }

    public class TaskStep
    {
        [Key]
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public bool IsCompleted { get; set; }
    }

    // 3. Intelligent Reminder System
    public class Reminder
    {
        [Key]
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Type { get; set; } // Soft, Email, Calendar
        public int EscalationLevel { get; set; } = 0; // 0: Gentle, 1: Repeated, 2: Escalated
        public bool IsAcknowledged { get; set; } = false;
    }

    // 4. Performance Insights (Weekly log)
    public class DailyInsightLog
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public int TasksCompleted { get; set; }
        public int FocusMinutes { get; set; }
    }
}
