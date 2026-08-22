using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessCreation;

public class BotSelectKeywordPack : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly BotActionHandlerService _botActionHandler;
    private readonly BotSearchDraftService _draftService;

    public BotSelectKeywordPack(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        BotActionHandlerService actionHandler,
        BotSearchDraftService draftService)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _botActionHandler = actionHandler;
        _draftService = draftService;
    }

    public string Action => "add_keyword_pack";

    public async Task<Message> HandleAsync(BotAction action)
    {
        Guid.TryParse(action.Data, out var keywordPackId);

        var draftProcess = await _draftService.GetDraftProcessAsync(action.UserId);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        draftProcess.ProcessInputState = ProcessInputState.Keywords;

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
