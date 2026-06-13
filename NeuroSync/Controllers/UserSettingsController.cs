using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Api.Data;
using NeuroSync.Api.Models;

namespace NeuroSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var s = await _db.UserSettings.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        if (s == null || string.IsNullOrWhiteSpace(s.PreferencesJson))
            return Content("{}", "application/json");
        return Content(s.PreferencesJson, "application/json");
    }

    [HttpPut]
    public async Task<IActionResult> Save([FromBody] JsonElement body)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var json = body.GetRawText();

        var s = await _db.UserSettings.FirstOrDefaultAsync(u => u.UserId == userId);
        if (s == null)
            _db.UserSettings.Add(new UserSettings { UserId = userId!, PreferencesJson = json });
        else
            s.PreferencesJson = json;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Settings saved." });
    }
}
