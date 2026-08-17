using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMessageHandler
{
    private readonly AppDbContext _dbContext;
    private readonly TelegramBotClient _bot;
    private readonly BotActionHandlerService _actionHandler;

    public BotMessageHandler(
        AppDbContext dbContext,
        TelegramBotClient bot,
        BotActionHandlerService actionHandler)
    {
        _dbContext = dbContext;
        _bot = bot;
        _actionHandler = actionHandler;
    }

    public async Task HandleAsync(Message message)
    {
        if (message.Text is null)
            return;

        if (message.Text == "/start")
        {
            await _actionHandler.HandleAction(new BotAction(
                message.From!.Id,
                "start",
                null,
                message.Chat.Id,
                null
            ));

            return;
        }

        var draftProcess = await GetDraftProcess(message.From!.Id);

        if (draftProcess is null)
            return;

        switch (draftProcess.ProcessInputState)
        {
            case ProcessInputState.Name:
                await HandleNameInput(message, draftProcess);
                break;

                // case ProcessInputState.Keywords:
                //     await HandleKeywordsInput(message, draftProcess);
                //     break;
                //
                // case ProcessInputState.ConfigName:
                //     await HandleConfigNameInput(message, draftProcess);
                //     break;
        }
    }

    private async Task HandleNameInput(Message message, Process draftProcess)
    {
        var text = message.Text!;
        var shortText = text.Length > 20 ? text[..20] : text;
        draftProcess.Name = shortText;
        draftProcess.ProcessInputState = ProcessInputState.None;
        await _dbContext.SaveChangesAsync();
        await _actionHandler.HandleAction(new BotAction(
                    message.From!.Id,
                    "select_group",
                    draftProcess.Id.ToString(),
                    message.Chat.Id,
                    null
                    ));
    }

    private async Task<Process?> GetDraftProcess(long userId)
    {
        return await _dbContext.Processes
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == userId &&
                p.ProcessStatus == ProcessStatus.Draft &&
                p.ProcessInputState != ProcessInputState.None);
    }
}
