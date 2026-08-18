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

            case ProcessInputState.Keywords:
                await HandleKeywordsInput(message, draftProcess);
                break;

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
                    "show_groups",
                    draftProcess.Id.ToString(),
                    message.Chat.Id,
                    null
                    ));
    }

    private async Task HandleKeywordsInput(Message message, Process draftProcess)
    {
        //TODO: handle input validation
        var keyword = message.Text!;

        draftProcess.Config.Keywords[keyword] = 0;

        await _dbContext.SaveChangesAsync();//TODO: commit and fix config's keyword(change it from jsonb)

        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                InlineKeyboardButton.WithCallbackData("1", $"keyword_weight:1"),
                InlineKeyboardButton.WithCallbackData("2", $"keyword_weight:2"),
                InlineKeyboardButton.WithCallbackData("3", $"keyword_weight:3"),
                InlineKeyboardButton.WithCallbackData("4", $"keyword_weight:4"),
                InlineKeyboardButton.WithCallbackData("5", $"keyword_weight:5")
                ]
            ]
        };

        await _bot.SendMessage(
            chatId: message.Chat.Id,
            text: $"کلمه «{keyword}» چه وزنی داشته باشد؟",
            replyMarkup: keyboard
        );
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
