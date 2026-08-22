using OppMonitor.Api.Data;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessManagement;

public class BotShowSearchesHandler : IBotActionHandler
{
    public string Action => "show_searches";

    private readonly TelegramBotClient _bot;
    private readonly BotKeyboardProvider _keyboardProvider;

    public BotShowSearchesHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider)
    {
        _bot = bot;
        _keyboardProvider = keyboardProvider;
    }

    public async Task<Message> HandleAsync(BotAction action)
    {
        return await _bot.SendMessage(
            action.ChatId,
            $"Your processes:",
            replyMarkup: await _keyboardProvider.GetProcessesKeyboard(action.UserId)
        );

    }
}
