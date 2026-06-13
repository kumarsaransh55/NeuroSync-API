using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Api.Data;
using NeuroSync.Api.DTOs;
using NeuroSync.Api.Models;
using NeuroSync.Api.Services;
using System.Security.Claims;

namespace NeuroSync.Api.Controllers;

[ApiController]
[Authorize]
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

    [HttpPost("create-task")]

    public async Task<IActionResult> CreateTaskFromText([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
            return BadRequest(new { message = "Please enter a task to break down." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        AiTaskBreakdownResponse aiResult;
        try
        {
            aiResult = await _ai.BreakTaskIntoMicroStepsAsync(request.RawText);
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "The AI service is busy right now. Please try again in a moment." });
        }

        var task = new TaskItem
        {
            UserId = userId!,
            OriginalSourceText = request.RawText,
            Title = aiResult.TaskTitle,
            Summary = aiResult.TaskSummary,
            ProjectId = request.ProjectId,
            TotalMinutes = 0 // Will update this in a second
        };

        int runningTotal = 0;

        for (int i = 0; i < aiResult.Steps.Count; i++)
        {
            var aiStep = aiResult.Steps[i];

            // Add to running total
            runningTotal += aiStep.EstimatedMinutes;

            task.MicroSteps.Add(new TaskStep
            {
                Heading = aiStep.Heading,
                Description = aiStep.Description,
                EstimatedMinutes = aiStep.EstimatedMinutes, // Integer!
                OrderIndex = i
            });
        }

        // Assign the calculated total
        task.TotalMinutes = runningTotal;

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return Ok(task);
    }

    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetTasks()
    {
        var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var tasks = await _db.Tasks
            .Include(t => t.MicroSteps)
            .Where(t => t.UserId == userIdFromToken)
            .ToListAsync();

        return Ok(tasks);
    }

    // Create a plain task (no AI breakdown) — used by the "Create Task" button.
    [HttpPost("quick")]
    public async Task<IActionResult> QuickCreate([FromBody] QuickCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "A task needs a title." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var task = new TaskItem
        {
            UserId = userId!,
            Title = request.Title,
            Summary = request.Description ?? string.Empty,
            OriginalSourceText = request.Title,
            TotalMinutes = 0
        };
        if (DateTime.TryParse(request.DueDate, out var quickDue)) task.SuggestedDeadline = quickDue;
        task.ProjectId = request.ProjectId;
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    // Update a task's title/summary/progress and replace its steps.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var task = await _db.Tasks
            .Include(t => t.MicroSteps)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task == null) return NotFound(new { message = "Task not found." });

        task.Title = request.Title ?? task.Title;
        task.Summary = request.Summary ?? task.Summary;
        task.ProgressPercentage = request.ProgressPercentage;
        if (!string.IsNullOrWhiteSpace(request.DueDate) && DateTime.TryParse(request.DueDate, out var due))
            task.SuggestedDeadline = due;
        task.ProjectId = request.ProjectId;

        // Replace the existing steps with the incoming set.
        var existing = task.MicroSteps.ToList();
        if (existing.Count > 0) _db.TaskSteps.RemoveRange(existing);
        task.MicroSteps.Clear();

        int total = 0;
        var steps = request.Steps ?? new List<TaskStepDto>();
        for (int i = 0; i < steps.Count; i++)
        {
            var s = steps[i];
            total += s.EstimatedMinutes;
            task.MicroSteps.Add(new TaskStep
            {
                Heading = s.Heading,
                Description = s.Description,
                EstimatedMinutes = s.EstimatedMinutes,
                OrderIndex = i,
                IsCompleted = s.IsCompleted
            });
        }
        task.TotalMinutes = total;

        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var task = await _db.Tasks
            .Include(t => t.MicroSteps)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task == null) return NotFound(new { message = "Task not found." });

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Task deleted." });
    }

    // AI breakdown ONLY (no DB write) — used to break an already-saved task into
    // steps. The caller then PUTs the returned steps onto the existing task,
    // so we never create a duplicate row.
    [HttpPost("breakdown")]
    public async Task<IActionResult> Breakdown([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
            return BadRequest(new { message = "Nothing to break down." });
        try
        {
            var result = await _ai.BreakTaskIntoMicroStepsAsync(request.RawText);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "The AI service is busy right now. Please try again in a moment." });
        }
    }
}