using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessCreation;

public class BotSelectWeightHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly BotActionHandlerService _botActionHandler;
    private readonly BotSearchDraftService _draftService;

    public BotSelectWeightHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        BotActionHandlerService botActionHandler,
        BotSearchDraftService draftService)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _botActionHandler = botActionHandler;
        _draftService = draftService;
    }

    public string Action => "keyword_weight";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var parts = action.Data!.Split(':');

        int.TryParse(parts[0], out var weight);
        Guid.TryParse(parts[1], out var keywordId);

        var draftProcess = await _draftService.GetDraftProcessAsync(action.UserId);

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
