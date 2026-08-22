using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessCreation;

public class BotSelectScoreThreshold : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly ProcessManager _processManager;

    public BotSelectScoreThreshold(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        BotActionHandlerService actionHandler,
        ProcessManager processManager)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _processManager = processManager;
    }

    public string Action => "select_score_threshold";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var draftProcess = await _processManager.GetDraftProcessAsync(action.UserId);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        draftProcess.ProcessInputState = ProcessInputState.ScoreThreshold;

        await _dbContext.SaveChangesAsync();

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            messageId: action.MessageId!.Value,
            text:
                "🎯 حداقل امتیاز را وارد کنید:\n\n" +
                "هرچه عدد بالاتر باشد، فقط پیام‌های مرتبط‌تر نمایش داده می‌شوند.\n\n" +
                "مثال: 10",
            replyMarkup: null
        );
    }
}
