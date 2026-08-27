namespace Sarnakh.Api.Dtos;

public class TelegramBatchDto
{
    public List<TelegramMessageDto>? Messages { get; set; }
    public string Status { get; set; } = "";
}
