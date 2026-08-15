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
        _bot.OnMessage += OnMessage;
        _bot.OnUpdate += OnUpdate;

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task OnUpdate(Update update)
    {
        var callback = update.CallbackQuery;

        if (callback?.Data is null || callback.Message is null)
            return Task.CompletedTask;

        var parts = callback.Data.Split(':');

        return HandleAction(new BotAction(
            callback.From.Id,
            parts[0],
            parts.Length > 1 ? parts[1] : null,
            callback.Message.Chat.Id,
            callback.Message.Id
        ));

    }
    private Task OnMessage(Message message, UpdateType type)
    {
        if (message.Text is null || message.Text != "/start")
            return Task.CompletedTask;

        return HandleAction(new BotAction(
            message.From!.Id,
            message.Text,
            null,
            message.Chat.Id,
         null
        ));
    }

    private async Task HandleAction(BotAction action)
    {
        using var scope = _scopeFactory.CreateScope();

        var handlers = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IBotHandler>>();

        var handler = handlers.FirstOrDefault(
                h => h.Action == action.Action);

        if (handler is not null)
            await handler.HandleAsync(action);
    }
}
