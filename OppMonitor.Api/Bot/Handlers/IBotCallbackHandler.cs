using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public interface IBotCallbackHandler
{
    string Action { get; }
    Task HandleAsync(CallbackQuery CallbackQuery);
}

