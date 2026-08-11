namespace OppMonitor.Api.Models;

public class Config
{
    public Guid Id { get; set; }
    //public Guid ApplicationUserId { get; set; }
    public Dictionary<string, int> Keywords { get; set; } = new();
    public List<long> TelegramChatIds { get; set; } = new();
    public int SearchDays { get; set; } = 7;
    public int ScoreThreshold { get; set; }
}
