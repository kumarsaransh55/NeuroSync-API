using Microsoft.AspNetCore.Mvc;
using NeuroSync.Api.Data;
using NeuroSync.Api.DTOs;
using NeuroSync.Api.Models;
using NeuroSync.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace NeuroSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAiAssistantService _ai;

    public TasksController(AppDbContext db, IAiAssistantService ai)
    {
        _db = db;
        _ai = ai;
    }

    [HttpPost("create-from-raw")]
    public async Task<IActionResult> CreateTaskFromText([FromBody] CreateTaskRequest request)
    {
        var steps = await _ai.BreakTaskIntoMicroStepsAsync(request.RawText);

        var task = new TaskItem
        {
            Title = "AI Generated Task",
            UserId = request.UserId,
            OriginalSourceText = request.RawText
        };

        for (int i = 0; i < steps.Count; i++)
        {
            task.MicroSteps.Add(new TaskStep { Description = steps[i], OrderIndex = i });
        }

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return Ok(task);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetTasks(string userId)
    {
        var tasks = await _db.Tasks.Include(t => t.MicroSteps).Where(t => t.UserId == userId).ToListAsync();
        return Ok(tasks);
    }
}