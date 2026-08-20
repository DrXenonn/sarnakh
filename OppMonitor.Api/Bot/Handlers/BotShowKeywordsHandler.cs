using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotShowKeywordsHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;

    public BotShowKeywordsHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
    }

    public string Action => "show_keywords";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var draftProcess = await _dbContext.Processes
            .Include(p => p.Config)
            .ThenInclude(p => p.Keywords)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        var keywords = draftProcess.Config!.Keywords;

        var keywordList = string.Join(
            "\n",
            keywords.Select(k => $"🔹 {k.Text} — وزن {k.Weight}")
        );

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            messageId: action.MessageId!.Value,
            text:
                "🔑 کلمات کلیدی:\n\n" +
                keywordList +
                "\n\n" +
                "➕ برای افزودن کلمه بعدی، آن را ارسال کنید.",
            replyMarkup: await _keyboardProvider.GetKeywordPacksWithDurationPick()
        );
    }
}
