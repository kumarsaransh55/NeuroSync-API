using Microsoft.AspNetCore.Mvc;
using NeuroSync.Api.DTOs;
using NeuroSync.Api.Services;

namespace NeuroSync.Api.Controllers;

[ApiController]
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
        var result = await _ai.SummarizeDocumentAsync(rawDocument);
        return Ok(result);
    }
}