using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OppMonitor.Api.Bot.Handlers;

public class BotSelectWeightHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;

    public BotSelectWeightHandler(
        TelegramBotClient bot,
        AppDbContext dbContext)
    {
        _bot = bot;
        _dbContext = dbContext;
    }

    public string Action => "keyword_weight";

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null || !int.TryParse(action.Data, out var weight))
            throw new InvalidOperationException("Invalid keyword weight.");

        if (weight < 1 || weight > 5)
            throw new InvalidOperationException("Keyword weight must be between 1 and 5.");

        var draftProcess = await _dbContext.Processes
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        // Find the keyword that hasn't received a weight yet.
        var keyword = draftProcess.Config.Keywords
            .FirstOrDefault(k => k.Weight == null);

        if (keyword is null)
            throw new InvalidOperationException("No keyword is waiting for a weight.");

        keyword.Weight = weight;

        await _dbContext.SaveChangesAsync();

        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        "➕ افزودن کلمه دیگر",
                        "add_keyword")
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        "📅 انتخاب تعداد روز",
                        "select_search_days")
                ]
            ]
        };

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            messageId: action.MessageId!.Value,
            text:
                $"✅ وزن کلمه «{keyword.Text}» روی {weight} تنظیم شد.\n\n" +
                "مرحله بعد را انتخاب کنید:",
            replyMarkup: keyboard
        );
    }
}
