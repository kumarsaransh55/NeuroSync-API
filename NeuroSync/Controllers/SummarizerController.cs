using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Api.Data;
using NeuroSync.Api.Services;
using System.Security.Claims;

namespace NeuroSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SummarizerController : ControllerBase
{
    private readonly IAiAssistantService _ai;
    private readonly AppDbContext _db;
    private readonly ILogger<SummarizerController> _logger;

    public SummarizerController(IAiAssistantService ai, AppDbContext db, ILogger<SummarizerController> logger)
    {
        _ai = ai;
        _db = db;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeText([FromBody] string rawDocument)
    {
        if (string.IsNullOrWhiteSpace(rawDocument))
            return BadRequest(new { message = "Please provide some text to analyze." });

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var settings = await _db.UserSettings.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            var personalization = PersonalizationHelper.Build(settings?.PreferencesJson);

            var result = await _ai.SummarizeDocumentAsync(rawDocument, personalization);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log the REAL error (status code, deployment-not-found, auth, etc.).
            // Visible in Azure App Service → Log stream / Application Insights.
            _logger.LogError(ex, "Summarizer AnalyzeText failed");
            return StatusCode(502, new { message = "The AI service is busy right now. Please try again in a moment." });
        }
    }
}
