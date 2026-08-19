using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMainmenuHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly BotKeyboardProvider _keyboardProvider;

    public BotMainmenuHandler(
        TelegramBotClient bot,
        BotKeyboardProvider keyboardProvider)
    {
        _bot = bot;
        _keyboardProvider = keyboardProvider;
    }

    public string Action => "start";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var keyboard = _keyboardProvider.GetMainMenuKeyboard();

        var text = "چه کاری می‌خواهید انجام دهید؟";

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
