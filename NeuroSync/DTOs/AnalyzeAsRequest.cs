using System.Text.Json;

namespace NeuroSync.Api.DTOs;

// Payload for POST /api/summarizer/analyze-as — analyze the SAME text under an
// explicit personalization profile (instead of the caller's saved settings).
// Powers the "see this email for a different profile" comparison, which makes
// the adaptive-output USP visible side by side.
public class AnalyzeAsRequest
{
    public string Text { get; set; } = string.Empty;

    // The personalization profile to apply, sent as the same camelCase JSON the
    // frontend stores in settings (simpleLanguage, stepSize, oneThingAtATime,
    // showTimeEstimates). Null/omitted = no personalization (the "standard" view).
    public JsonElement? Profile { get; set; }
}
