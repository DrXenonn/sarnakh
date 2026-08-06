namespace OppMonitor.Api.Models;

public class Lead
{
    public string Message { get; set; } = string.Empty;
    public int Score { get; set; }

    //TODO add message time, sender ID, message link, etc.
}
