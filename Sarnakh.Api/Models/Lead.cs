using NodaTime;

namespace Sarnakh.Api.Models;

public class Lead
{
    public Guid Id { get; set; }
    public int TelegramMessageId { get; set; }
    public long? SenderId { get; set; }
    public string? Username { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Score { get; set; }
    public Instant MessageDate { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public long GroupId { get; set; }
    public Guid ProcessId { get; set; }
}
