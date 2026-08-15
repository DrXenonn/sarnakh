using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public interface IBotHandler
{
    string Action { get; }
    Task<Message> HandleAsync(BotAction action);
}

public record BotAction(
        long UserId,
        string Action,
        string? Data,
        long ChatId,
        int? MessageId
        );
