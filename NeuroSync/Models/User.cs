using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.Models;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Unique string ID
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // NEVER store plain text
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}