using NodaTime;
using OppMonitor.Api.Models;

namespace OppMonitor.Api.Pipeline;

public class TelegramMessageContext
{
    // Raw input (set once at creation)
    public long MessageId { get; init; }
    public string Text { get; init; } = string.Empty;
    public string? Username { get; init; }
    public long? SenderId { get; init; }
    public long GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public Instant Date { get; init; }
    public Process Process { get; set; } = null!;

    // Accumulated by steps
    public List<string> MatchedKeywords { get; set; } = [];
    public int Score { get; set; }
    public bool IsLead { get; set; }

    // Optional metadata for debugging
    public string? DropReason { get; set; }
}
