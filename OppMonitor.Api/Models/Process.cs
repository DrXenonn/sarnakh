namespace OppMonitor.Api.Models;

public class Process
{
    public Guid Id { get; set; }
    // public Guid ApplicationUserId {get; set;}  auth
    public ProcessStatus ProcessStatus { get; set; }
    public Config Config { get; set; } = new();
}

public enum ProcessStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Terminated
}
