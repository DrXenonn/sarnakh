using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotSelectKeywordPack : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly BotActionHandlerService _botActionHandler;

    public BotSelectKeywordPack(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        BotActionHandlerService actionHandler)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _botActionHandler = actionHandler;
    }

    public string Action => "add_keyword_pack";

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null)
            throw new InvalidOperationException("No data for this action.");

        if (!Guid.TryParse(action.Data, out var keywordPackId))
            throw new InvalidOperationException("Invalid id.");

        var draftProcess = await _dbContext.Processes
            .Include(p => p.Config)
            .ThenInclude(p => p.Keywords)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        draftProcess.ProcessInputState = ProcessInputState.Keywords;

        Console.WriteLine(keywordPackId);

        var keywordPack = await _dbContext.KeywordPacks
            .Include(p => p.Keywords)
            .FirstOrDefaultAsync(p => p.Id == keywordPackId);

        if (keywordPack is not null)
            foreach (var packKeyword in keywordPack.Keywords)
            {
                _dbContext.Keywords.Add(new Keyword
                {
                    Id = Guid.NewGuid(),
                    Text = packKeyword.Text,
                    Weight = packKeyword.Weight,
                    ConfigId = draftProcess.Config!.Id
                });
            }

        await _dbContext.SaveChangesAsync();

        return await _botActionHandler.HandleAction(
            new BotAction(
                action.UserId,
                "show_keywords",
                null,
                action.ChatId,
                action.MessageId
            )
        );
    }
}
