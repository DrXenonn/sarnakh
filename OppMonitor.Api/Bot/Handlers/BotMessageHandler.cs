using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMessageHandler
{
    private readonly AppDbContext _dbContext;
    private readonly TelegramBotClient _bot;
    private readonly BotActionHandlerService _actionHandler;
    private readonly BotKeyboardProvider _keyboardProvider;

    public BotMessageHandler(
        AppDbContext dbContext,
        TelegramBotClient bot,
        BotActionHandlerService actionHandler,
        BotKeyboardProvider keyboardProvider)
    {
        _dbContext = dbContext;
        _bot = bot;
        _actionHandler = actionHandler;
        _keyboardProvider = keyboardProvider;
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

            case ProcessInputState.ScoreThreshold:
                await HandleScoreThresholdInput(message, draftProcess);
                break;
            case ProcessInputState.SearchDays:
                await HandleSearchDaysInput(message, draftProcess);
                break;
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
        var keyword = message.Text!;
        var id = Guid.NewGuid();

        var newKeyword = new Keyword
        {
            Id = id,
            Text = keyword,
            Weight = null,
            ConfigId = draftProcess.Config!.Id
        };

        _dbContext.Keywords.Add(newKeyword);
        draftProcess.ProcessInputState = ProcessInputState.None;

        await _dbContext.SaveChangesAsync();

        var keyboard = _keyboardProvider.GetKeywordWeightKeyboard(id);

        await _bot.SendMessage(
            chatId: message.Chat.Id,
            text: $"🔑 اهمیت کلمه «{keyword}» را انتخاب کنید:",
            replyMarkup: keyboard
        );
    }

    private async Task HandleScoreThresholdInput(Message message, Process draftProcess)
    {
        if (!int.TryParse(message.Text, out var threshold))
        {
            await _bot.SendMessage(
                message.Chat.Id,
                "لطفاً یک عدد وارد کنید."
            );
            return;
        }

        draftProcess.Config!.ScoreThreshold = threshold;
        draftProcess.ProcessInputState = ProcessInputState.None;

        await _dbContext.SaveChangesAsync();

        await _actionHandler.HandleAction(
            new BotAction(
                message.From!.Id,
                "select_search_days",
                null,
                message.Chat.Id,
                null
            )
        );
    }

    private async Task HandleSearchDaysInput(
        Message message,
        Process draftProcess)
    {
        if (!int.TryParse(message.Text, out var days) ||
            days < 1 ||
            days > 365)
        {
            await _bot.SendMessage(
                message.Chat.Id,
                "لطفاً تعداد روز را به صورت عددی بین ۱ تا ۳۶۵ وارد کنید."
            );

            return;
        }

        draftProcess.Config!.SearchDays = days;
        draftProcess.ProcessInputState = ProcessInputState.None;

        await _dbContext.SaveChangesAsync();

        await _actionHandler.HandleAction(
            new BotAction(
                message.From!.Id,
                "show_summary",
                null,
                message.Chat.Id,
                null
            )
        );
    }

    private async Task<Process?> GetDraftProcess(long userId)
    {
        return await _dbContext.Processes
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == userId &&
                p.ProcessStatus == ProcessStatus.Draft &&
                p.ProcessInputState != ProcessInputState.None);
    }
}
