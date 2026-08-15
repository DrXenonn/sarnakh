using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OppMonitor.Api.Bot.Handlers;

public class BotNewSearchHandler : IBotHandler
{
    private readonly TelegramBotClient _bot;

    public BotNewSearchHandler(TelegramBotClient bot)
    {
        _bot = bot;
    }

    public string Action => "new_search";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        "💄 زیبایی و مراقبت",
                        "category:beauty"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        "⚽ ورزش",
                        "category:sports"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        "🏠 خانه و آشپزی",
                        "category:home"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        "🚗 خودرو",
                        "category:cars"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        "🔙 بازگشت",
                        "/start"
                    )
                ]
            ]
        };

        const string text =
            "🔎 جستجوی جدید\n\n" +
            "دسته‌بندی موردنظر خود را انتخاب کنید:";

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
