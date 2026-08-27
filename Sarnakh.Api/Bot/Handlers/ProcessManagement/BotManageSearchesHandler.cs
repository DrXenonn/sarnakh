using Microsoft.EntityFrameworkCore;
using Sarnakh.Api.Data;
using Sarnakh.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sarnakh.Api.Bot.Handlers.ProcessManagement;

public class BotManageSearchesHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly ProcessManager _processManager;

    public BotManageSearchesHandler(
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

    public string Action => "manage_search";

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null || !Guid.TryParse(action.Data, out var processId))
            throw new InvalidOperationException("Invalid process ID.");

        var process = await _processManager.GetProcessAsync(processId);

        if (process is null)
            throw new InvalidOperationException("Process not found.");

        if (process.TelegramUserId != action.UserId)
            throw new InvalidOperationException("Process does not belong to this user.");

        var leadsCount = await _dbContext.Leads
            .CountAsync(l => l.ProcessId == processId);

        var config = process.Config!;

        var status = _processManager.GetStatusText(process.ProcessStatus);

        var text =
            $"📊 جستجو: {process.Name}\n\n" +
            $"📌 وضعیت: {status}\n\n" +
            $"📈 نتایج:\n" +
            $"   • {leadsCount} سرنخ پیدا شده\n\n" +
            $"⚙️ تنظیمات:\n" +
            $"   • {config.Keywords.Count} کلمه کلیدی\n" +
            $"   • {config.TelegramChatIds.Count} گروه\n" +
            $"   • حداقل امتیاز: {config.ScoreThreshold}\n" +
            $"   • بازه جستجو: {config.SearchDays} روز گذشته\n\n";

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            messageId: action.MessageId!.Value,
            text: text,
            replyMarkup: _keyboardProvider
                .GetProcessManagementKeyboard(process)
        );
    }
}
