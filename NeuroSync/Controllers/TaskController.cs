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
}