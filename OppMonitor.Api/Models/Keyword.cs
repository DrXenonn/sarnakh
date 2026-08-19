namespace OppMonitor.Api.Models;

public class Keyword
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? Weight { get; set; }

    // For user/process config keywords
    public Guid? ConfigId { get; set; }
    public Config? Config { get; set; }

    // For premade pack keywords
    public Guid? KeywordPackId { get; set; }
    public KeywordPack? KeywordPack { get; set; }
}
