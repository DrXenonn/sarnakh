using Sarnakh.Api.Data;
using Sarnakh.Api.Models;
using Sarnakh.Api.Services;
using Telegram.Bot.Types;
using Microsoft.EntityFrameworkCore;

namespace Sarnakh.Api.Bot.Handlers.ProcessManagement;

public class BotRerunSearchHandler : IBotActionHandler
{
    public string Action => "rerun_search";

    private readonly AppDbContext _dbContext;
    private readonly ProcessManager _processManager;
    private readonly RedisCommandQueueService _redisCommandQueueService;
    private readonly BotActionHandlerService _actionHandler;

    public BotRerunSearchHandler(
        AppDbContext dbContext,
        ProcessManager draftService,
        RedisCommandQueueService commandQueueService,
        BotActionHandlerService actionHandler)
    {
        _dbContext = dbContext;
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

        var leads = await _dbContext.Leads
        .Where(l => l.ProcessId == process.Id).ToListAsync();
        _dbContext.Leads.RemoveRange(leads);

        process.ProcessStatus = ProcessStatus.Pending;

        await _redisCommandQueueService
            .QueueScanHistoricalAsync(
                    process.Id,
                    process.Config.TelegramChatIds,
                    process.Config.SearchDays);

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
