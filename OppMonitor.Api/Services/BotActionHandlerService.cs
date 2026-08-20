using OppMonitor.Api.Bot.Handlers;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Services;

public class BotActionHandlerService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BotActionHandlerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<Message> HandleAction(BotAction action)
    {
        using var scope = _scopeFactory.CreateScope();

        var handlers = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IBotActionHandler>>();

        var handler = handlers.FirstOrDefault(
            h => h.Action == action.Action);

        if (handler is null)
            throw new InvalidOperationException(
                $"No handler found for action: {action.Action}");

        return await handler.HandleAsync(action);
    }
}
