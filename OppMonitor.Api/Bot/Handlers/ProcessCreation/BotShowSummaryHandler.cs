using OppMonitor.Api.Data;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessCreation;

public class BotShowSummaryHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly ProcessManager _processManager;

    public BotShowSummaryHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider,
        ProcessManager processManager)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
        _processManager = processManager;
    }

    public string Action => "show_summary";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var draftProcess = await _processManager.GetDraftProcessAsync(action.UserId);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        var config = draftProcess.Config!;

        var keywords = string.Join(
            "\n",
            config.Keywords.Select(k =>
                $"🔹 {k.Text} — وزن {k.Weight}")
        );

        return await _bot.SendMessage(
            action.ChatId,
            $"📋 خلاصه جستجو\n\n" +
            $"🔎 نام: {draftProcess.Name}\n\n" +
            $"🔑 کلمات:\n{keywords}\n\n" +
            $"🎯 حداقل امتیاز: {config.ScoreThreshold}\n" +
            $"⏳ بازه جستجو: {config.SearchDays} روز اخیر",
            replyMarkup: _keyboardProvider.GetSummaryKeyboard()
        );
    }
}
