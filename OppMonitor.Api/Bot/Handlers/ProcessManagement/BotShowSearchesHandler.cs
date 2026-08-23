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
        BotKeyboardProvider keyboardProvider)
    {
        _bot = bot;
        _keyboardProvider = keyboardProvider;
    }

    public async Task<Message> HandleAsync(BotAction action)
    {
        var keyboard = await _keyboardProvider
            .GetProcessesKeyboard(action.UserId);

        const string text = "📋 جستجوهای شما:";

        if (action.MessageId is not null)
        {
            return await _bot.EditMessageText(
                chatId: action.ChatId,
                messageId: action.MessageId.Value,
                text: text,
                replyMarkup: keyboard
            );
        }

        return await _bot.SendMessage(
            chatId: action.ChatId,
            text: text,
            replyMarkup: keyboard
        );
    }
}
