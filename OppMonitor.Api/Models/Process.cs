namespace OppMonitor.Api.Models;

public class Process
{
    public Guid Id { get; set; }
    public long TelegramUserId { get; set; }
    public ProcessStatus ProcessStatus { get; set; }
    public Guid ConfigId { get; set; }
    public Config Config { get; set; } = null!;
}

public enum ProcessStatus
{
    Pending,
    Running,
    Completed,
    Stopped,
    Failed
}
