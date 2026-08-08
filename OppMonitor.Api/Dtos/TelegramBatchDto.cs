namespace OppMonitor.Api.Dtos;

public class TelegramBatchDto
{
    public List<TelegramMessageDto>? Messages { get; set; }
    public TelegramMessageDto? Message { get; set; } // for "listen" single-message payloads
}
