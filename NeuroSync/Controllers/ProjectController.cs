using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Api.Data;
using NeuroSync.Api.DTOs;
using NeuroSync.Api.Models;
using System.Security.Claims;

namespace NeuroSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var projects = await _db.Projects
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
        return Ok(projects);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "A project needs a name." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var project = new Project
        {
            UserId = userId!,
            Name = request.Name.Trim(),
            ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? "#166534" : request.ColorHex
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return Ok(project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (project == null) return NotFound(new { message = "Project not found." });

        if (!string.IsNullOrWhiteSpace(request.Name)) project.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.ColorHex)) project.ColorHex = request.ColorHex;
        await _db.SaveChangesAsync();
        return Ok(project);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var project = await _db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (project == null) return NotFound(new { message = "Project not found." });

        // Don't delete the tasks — just unassign them so nothing is lost.
        foreach (var t in project.Tasks) t.ProjectId = null;
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Project deleted." });
    }
}
