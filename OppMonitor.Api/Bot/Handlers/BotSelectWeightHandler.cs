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
        if (action.Data is null)
            throw new InvalidOperationException("No data for this action.");

        var parts = action.Data.Split(':');

        if (parts.Length != 2)
            throw new InvalidOperationException("Invalid keyword weight data.");

        if (!int.TryParse(parts[0], out var weight))
            throw new InvalidOperationException("Invalid weight.");

        if (!Guid.TryParse(parts[1], out var keywordId))
            throw new InvalidOperationException("Invalid keyword ID.");

        if (weight < 1 || weight > 5)
            throw new InvalidOperationException(
                "Keyword weight must be between 1 and 5.");

        var draftProcess = await _dbContext.Processes
            .Include(p => p.Config)
            .ThenInclude(p => p.Keywords)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        var keyword = draftProcess.Config!.Keywords
            .Find(k => k.Id == keywordId);

        if (keyword is null)
            throw new InvalidOperationException("Keyword not found.");

        keyword.Weight = weight;

        draftProcess.ProcessInputState = ProcessInputState.Keywords;

        await _dbContext.SaveChangesAsync();

        var keywords = draftProcess.Config!.Keywords;

        var keywordList = string.Join(
            "\n",
            keywords.Select(k => $"🔹 {k.Text} — وزن {k.Weight}")
        );

        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
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
                "🔑 کلمات کلیدی:\n\n" +
                keywordList +
                "\n\n" +
                "➕ برای افزودن کلمه بعدی، آن را ارسال کنید.",
            replyMarkup: keyboard
        );
    }
}
