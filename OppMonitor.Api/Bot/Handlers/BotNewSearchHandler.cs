using OppMonitor.Api.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;

namespace OppMonitor.Api.Bot.Handlers;

public class BotNewSearchHandler : IBotHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;

    public BotNewSearchHandler(
        TelegramBotClient bot,
        AppDbContext dbContext)
    {
        _bot = bot;
        _dbContext = dbContext;
    }

    public string Action => "new_search";

    //TODO: we create an empty process with Draft status and populate it during these steps.

    public async Task<Message> HandleAsync(BotAction action)
    {
        var groupCategories = await _dbContext.TelegramGroupCategories.ToListAsync();

        var buttons = new List<IEnumerable<InlineKeyboardButton>>();

        foreach (var groupCategory in groupCategories)
        {
            var button = InlineKeyboardButton
                .WithCallbackData(
                        $"{groupCategory.Name} ({groupCategory.GroupIds.Count} گروه)",
                        groupCategory.Id.ToString());
            buttons.Add([button]);
        }

        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
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
