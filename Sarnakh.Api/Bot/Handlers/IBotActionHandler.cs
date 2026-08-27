using Telegram.Bot.Types;

namespace Sarnakh.Api.Bot.Handlers;

public interface IBotActionHandler
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
