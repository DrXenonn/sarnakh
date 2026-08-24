using OppMonitor.Api.Services;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessManagement;

public class BotStopSearchHandler : IBotActionHandler
{
    public string Action => "stop_search";

    private readonly ProcessManager _processManager;
    private readonly RedisCommandQueueService _redisCommandQueueService;
    private readonly BotActionHandlerService _actionHandler;

    public BotStopSearchHandler(
        ProcessManager draftService,
        RedisCommandQueueService commandQueueService,
        BotActionHandlerService actionHandler)
    {
        _processManager = draftService;
        _redisCommandQueueService = commandQueueService;
        _actionHandler = actionHandler;
    }

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null || !Guid.TryParse(action.Data, out var processId))
            throw new InvalidOperationException("Invalid process ID.");

        var process = await _processManager.GetProcessAsync(processId);
        if (process is null)
            throw new InvalidOperationException("Process not found.");

        await _redisCommandQueueService.QueueStopScanAsync(processId);

        return await _actionHandler.HandleAction(new BotAction(
                    action.UserId,
                    "show_searches",
                    null,
                    action.ChatId,
                    action.MessageId
                    ));
    }
}
