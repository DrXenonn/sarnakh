namespace Sarnakh.Api.Models;

public class TelegramGroupCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<long> GroupIds { get; set; } = new();
}
