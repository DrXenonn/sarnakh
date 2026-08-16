using OppMonitor.Api.Bot.Handlers;

namespace OppMonitor.Api.Services;

public class BotActionHandlerService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BotActionHandlerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task HandleAction(BotAction action)
    {
        using var scope = _scopeFactory.CreateScope();

        var handlers = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IBotActionHandler>>();

        var handler = handlers.FirstOrDefault(
                h => h.Action == action.Action);

        if (handler is not null)
            await handler.HandleAsync(action);
    }
}
