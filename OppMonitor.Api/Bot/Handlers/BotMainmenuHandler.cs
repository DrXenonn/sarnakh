using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMainmenuHandler : IBotHandler
{
    private readonly TelegramBotClient _bot;

    public BotMainmenuHandler(TelegramBotClient bot)
    {
        _bot = bot;
    }

    public string Action => "/start";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [("🔎 جستجوی جدید", "new_search")],
                [("📋 جستجوهای من", "searches")]
            ]
        };

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
