namespace NeuroSync.Api.DTOs;

// Payload for POST /api/task/from-actions — converts a set of action items
// (surfaced by the summarizer) into a single task with one subtask per item.
public class TaskFromActionsRequest
{
    // The original document/email text the action items came from (optional —
    // used to give the AI context when writing each step's description).
    public string? RawText { get; set; }

    // The action items to turn into subtasks. Kept verbatim and in order.
    public List<string> ActionItems { get; set; } = new();

    // Optional project to file the new task under.
    public int? ProjectId { get; set; }
}
