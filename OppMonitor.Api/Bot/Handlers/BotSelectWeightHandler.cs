using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotSelectWeightHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly BotActionHandlerService _botActionHandler;

    public BotSelectWeightHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        BotActionHandlerService botActionHandler)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _botActionHandler = botActionHandler;
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

        return await _botActionHandler.HandleAction(new BotAction(
                    action.UserId,
                    "show_keywords",
                    null,
                    action.ChatId,
                    action.MessageId));
    }
}
