namespace OppMonitor.Api.Dtos;

public record ProcessConfigDto(
    List<long> TelegramChatIds,
    Dictionary<string, int> Keywords,
    int SearchDays
);
