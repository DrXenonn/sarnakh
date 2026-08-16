namespace OppMonitor.Api.Models;

public class Process
{
    public Guid Id { get; set; }
    public long TelegramUserId { get; set; }
    public ProcessStatus ProcessStatus { get; set; }
    public Guid? ConfigId { get; set; }
    public Config? Config { get; set; }
    public ProcessInputState ProcessInputState { get; set; }
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
    ConfigName
}
