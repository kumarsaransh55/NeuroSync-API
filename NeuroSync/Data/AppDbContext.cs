using Microsoft.EntityFrameworkCore;
using NeuroSync.Api.Models;
using System.Collections.Generic;

namespace NeuroSync.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserSettings> UserSettings { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<TaskStep> TaskSteps { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
}