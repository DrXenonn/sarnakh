using OppMonitor.Api.Data;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotStartProcessHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly BotSearchDraftService _draftService;
    private readonly RedisCommandQueueService _redisCommandQueueService;

    public BotStartProcessHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        BotSearchDraftService draftService,
        RedisCommandQueueService commandQueueService)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _draftService = draftService;
        _redisCommandQueueService = commandQueueService;
    }

    public string Action => "start_process";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var draftProcess = await _draftService.GetDraftProcessAsync(action.UserId);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        await _redisCommandQueueService
            .QueueScanHistoricalAsync(
                    draftProcess.Id,
                    draftProcess.Config.TelegramChatIds,
                    draftProcess.Config.SearchDays);
        draftProcess.ProcessStatus = Models.ProcessStatus.Pending;


        return await _bot.SendMessage(
            action.ChatId,
            $"Process started"
        );
    }
}
