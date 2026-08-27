using Sarnakh.Api.Data;
using Sarnakh.Api.Models;
using Sarnakh.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sarnakh.Api.Bot.Handlers.ProcessCreation;

public class BotStartProcessHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly ProcessManager _processManager;
    private readonly RedisCommandQueueService _redisCommandQueueService;
    private readonly BotActionHandlerService _actionHandler;

    public BotStartProcessHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        ProcessManager draftService,
        RedisCommandQueueService commandQueueService,
        BotActionHandlerService actionHandler)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _processManager = draftService;
        _redisCommandQueueService = commandQueueService;
        _actionHandler = actionHandler;
    }

    public string Action => "start_process";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var draftProcess = await _processManager.GetDraftProcessAsync(action.UserId);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        draftProcess.ProcessStatus = ProcessStatus.Pending;

        await _redisCommandQueueService
            .QueueScanHistoricalAsync(
                    draftProcess.Id,
                    draftProcess.Config.TelegramChatIds,
                    draftProcess.Config.SearchDays);

        await _dbContext.SaveChangesAsync();

        return await _actionHandler.HandleAction(new BotAction(
                    action.UserId,
                    "show_searches",
                    null,
                    action.ChatId,
                    action.MessageId
                    ));
    }
}
