using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotAddKeywordPack : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly BotShowKeywordsHandler _showKeywordHandler;

    public BotAddKeywordPack(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        BotShowKeywordsHandler showKeyword)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _showKeywordHandler = showKeyword;
    }

    public string Action => "add_keyword_pack";

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null)
            throw new InvalidOperationException("No data for this action.");

        var parts = action.Data.Split(':');

        if (!int.TryParse(parts[0], out var keywordPackId))
            throw new InvalidOperationException("Invalid Id for keyword pack.");

        var draftProcess = await _dbContext.Processes
            .Include(p => p.Config)
            .ThenInclude(p => p.Keywords)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        var keywordPack = await _dbContext.KeywordPacks.FindAsync(keywordPackId);

        if (keywordPack is not null)
            await _dbContext.Keywords.AddRangeAsync(keywordPack.Keywords);

        await _dbContext.SaveChangesAsync();

        return await _showKeywordHandler.HandleAsync(new BotAction(action.UserId, "show_keywords", action.Data, action.ChatId, action.MessageId));
    }
}
