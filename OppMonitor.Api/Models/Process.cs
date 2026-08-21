namespace OppMonitor.Api.Models;

public class Process
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long TelegramUserId { get; set; }
    public ProcessStatus ProcessStatus { get; set; }
    public Guid ConfigId { get; set; }
    public Config Config { get; set; } = new();
    public ProcessInputState ProcessInputState { get; set; } = ProcessInputState.None;
}

public enum ProcessStatus
{
    Draft,
    Pending,
    Running,
    Completed,
    Stopped,
    Failed
}

public enum ProcessInputState
{
    None,
    Name,
    Keywords,
    ScoreThreshold,
    ConfigName
}
