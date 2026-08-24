using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessManagement;

public class BotDeleteSearchHandler : IBotActionHandler
{
    public string Action => "delete_search";

    private readonly TelegramBotClient _bot;
    private readonly ProcessManager _processManager;
    private readonly BotActionHandlerService _actionHandler;

    public BotDeleteSearchHandler(
        TelegramBotClient bot,
        ProcessManager processManager,
        BotActionHandlerService actionHanlder)
    {
        _bot = bot;
        _processManager = processManager;
        _actionHandler = actionHanlder;
    }

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null || !Guid.TryParse(action.Data, out var processId))
            throw new InvalidOperationException("Invalid process ID.");

        await _processManager.DeleteProcessAsync(processId, action.UserId);

        return await _actionHandler.HandleAction(new BotAction(
                    action.UserId,
                    "show_searches",
                    null,
                    action.ChatId,
                    null
                    ));
    }
}
