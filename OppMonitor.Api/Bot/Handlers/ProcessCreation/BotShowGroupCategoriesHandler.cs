using OppMonitor.Api.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Bot.Handlers.ProcessCreation;

public class BotShowGroupCategoriesHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;

    public BotShowGroupCategoriesHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
    }

    public string Action => "show_groups";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var keyboard = await _keyboardProvider.GetGroupCategoriesKeyboard();

        const string text =
            "👥 انتخاب حوزه جستجو\n\n" +
            "گروه‌های تلگرامی مرتبط در حوزه‌های مختلف توسط تیم سرنخ شناسایی و دسته‌بندی شده‌اند.\n\n" +
            "حوزه موردنظر خود را انتخاب کنید:";

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
