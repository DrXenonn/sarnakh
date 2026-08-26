namespace OppMonitor.Api.Models;

public class Config
{
    public Guid Id { get; set; }
    public List<Keyword> Keywords { get; set; } = [];
    public List<Guid> KeywordPackIds { get; set; } = [];
    public List<long> TelegramChatIds { get; set; } = [];
    public int SearchDays { get; set; } = 7;
    public int ScoreThreshold { get; set; }
}
