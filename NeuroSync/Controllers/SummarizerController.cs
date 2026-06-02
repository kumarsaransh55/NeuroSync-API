using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroSync.Api.DTOs;
using NeuroSync.Api.Services;

namespace NeuroSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SummarizerController : ControllerBase
{
    private readonly IAiAssistantService _ai;

    public SummarizerController(IAiAssistantService ai)
    {
        _ai = ai;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeText([FromBody] string rawDocument)
    {
        if (string.IsNullOrWhiteSpace(rawDocument))
            return BadRequest(new { message = "Please provide some text to analyze." });

        try
        {
            var result = await _ai.SummarizeDocumentAsync(rawDocument);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "The AI service is busy right now. Please try again in a moment." });
        }
    }
}