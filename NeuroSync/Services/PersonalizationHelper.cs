using System.Text.Json;

namespace NeuroSync.Api.Services;

// Turns a user's saved preferences (JSON) into a short instruction that gets
// appended to every AI prompt — so the SAME content adapts to how they think.
public static class PersonalizationHelper
{
    public static string Build(string? prefsJson)
    {
        if (string.IsNullOrWhiteSpace(prefsJson)) return "";
        try
        {
            using var doc = JsonDocument.Parse(prefsJson);
            var root = doc.RootElement;
            // Preferences may be top-level or nested under "profile".
            JsonElement p = root.TryGetProperty("profile", out var pr) && pr.ValueKind == JsonValueKind.Object ? pr : root;

            bool Flag(string key) =>
                p.TryGetProperty(key, out var v) &&
                (v.ValueKind == JsonValueKind.True ||
                 (v.ValueKind == JsonValueKind.String && string.Equals(v.GetString(), "true", StringComparison.OrdinalIgnoreCase)));
            string Str(string key) => p.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? (v.GetString() ?? "") : "";

            var lines = new List<string>();
            if (Flag("simpleLanguage"))
                lines.Add("Use very short sentences and plain, everyday words. Prefer bullet points over long paragraphs.");

            var step = Str("stepSize");
            if (step == "small") lines.Add("Break work into many SMALL steps; keep each step to about 10-15 minutes.");
            else if (step == "large") lines.Add("Use a few larger, consolidated steps rather than many tiny ones.");

            if (Flag("oneThingAtATime")) lines.Add("Each step must be a single, clear action — one thing at a time.");
            if (Flag("showTimeEstimates")) lines.Add("Always include a realistic time estimate for each step.");

            if (lines.Count == 0) return "";
            return "\n\n        Personalization for THIS user (follow these closely):\n        - " + string.Join("\n        - ", lines);
        }
        catch
        {
            return "";
        }
    }
}
