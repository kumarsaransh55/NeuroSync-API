using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Api.Data;
using NeuroSync.Api.DTOs;
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

    // Analyze the SAME text under an EXPLICIT profile (not the saved settings) so
    // the frontend can show one email adapted to two different profiles at once.
    [HttpPost("analyze-as")]
    public async Task<IActionResult> AnalyzeAs([FromBody] AnalyzeAsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Please provide some text to analyze." });

        try
        {
            // The profile arrives as the same camelCase JSON the helper already
            // understands, so we pass it straight through. No profile = standard.
            var personalization = request.Profile.HasValue
                ? PersonalizationHelper.Build(request.Profile.Value.GetRawText())
                : string.Empty;

            var result = await _ai.SummarizeDocumentAsync(request.Text, personalization);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Summarizer AnalyzeAs failed");
            return StatusCode(502, new { message = "The AI service is busy right now. Please try again in a moment." });
        }
    }
}
