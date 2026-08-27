using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Sarnakh.Api.Bot.Handlers;
using Sarnakh.Api.Services;

namespace Sarnakh.Api.Jobs;

public class OperatorBotJob : BackgroundService
{
    private readonly TelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BotActionHandlerService _actionHandler;

    public OperatorBotJob(
        TelegramBotClient bot,
        IServiceScopeFactory scopeFactory,
        BotActionHandlerService actionHandler)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
        _actionHandler = actionHandler;
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


        var parts = callback.Data.Split(':', 2);

        _bot.AnswerCallbackQuery(callback.Id);
        return _actionHandler.HandleAction(new BotAction(
            callback.From.Id,
            parts[0],
            parts.Length > 1 ? parts[1] : null,
            callback.Message.Chat.Id,
            callback.Message.Id
        ));
    }

    private Task OnMessage(Message message, UpdateType type)
    {
        if (message.Text is null)
            return Task.CompletedTask;

        return HandleMessage(message);
    }

    private async Task HandleMessage(Message message)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<BotMessageHandler>();
        await handler.HandleAsync(message);
    }
}
