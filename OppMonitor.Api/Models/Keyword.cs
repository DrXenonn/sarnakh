namespace OppMonitor.Api.Models;

public class Keyword
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? Weight { get; set; }
    public Guid ConfigId { get; set; }
    public Config Config { get; set; } = null!;
}
