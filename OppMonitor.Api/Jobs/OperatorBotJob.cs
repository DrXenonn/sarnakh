using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using OppMonitor.Api.Bot.Handlers;

namespace OppMonitor.Api.Jobs;

public class OperatorBotJob : BackgroundService
{
    private readonly TelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;

    public OperatorBotJob(
        TelegramBotClient bot,
        IServiceScopeFactory scopeFactory)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // _bot.OnMessage += OnMessage;
        _bot.OnUpdate += OnUpdate;

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnUpdate(Update update)
    {
        if (update.CallbackQuery is null)
            return;

        using var scope = _scopeFactory.CreateScope();

        var handlers = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IBotCallbackHandler>>();

        var action = update.CallbackQuery.Data;
        Console.WriteLine("got the action: " + action);

        var handler = handlers.FirstOrDefault(h => h.Action == action);

        if (handler != null)
            await handler.HandleAsync(update.CallbackQuery);
    }

    // private Task OnMessage(Message msg, UpdateType type)
    // {
    //     if (msg.Text != "/start")
    //         return;
    //
    // }
}
