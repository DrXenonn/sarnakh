namespace Sarnakh.Api.Models;

public class KeywordPack
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Keyword> Keywords { get; set; } = [];
}
