using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Sarnakh.Api.Data;
using Sarnakh.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sarnakh.Api.Bot.Handlers.ProcessManagement;

public class BotShowLeadsHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly ProcessManager _processManager;

    public BotShowLeadsHandler(
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

    public string Action => "show_leads";

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null)
            throw new InvalidOperationException("Missing lead navigation data.");

        var parts = action.Data.Split(':');

        if (parts.Length != 2)
            throw new InvalidOperationException("Invalid lead navigation data.");

        if (!Guid.TryParse(parts[0], out var processId))
            throw new InvalidOperationException("Invalid process ID.");

        if (!int.TryParse(parts[1], out var index) || index < 0)
            throw new InvalidOperationException("Invalid lead index.");

        var process = await _processManager.GetProcessAsync(processId);

        if (process is null)
            throw new InvalidOperationException("Process no longer exists.");

        if (process.TelegramUserId != action.UserId)
            throw new InvalidOperationException("You do not own this process.");

        var totalLeads = await _dbContext.Leads
            .CountAsync(l => l.ProcessId == processId);

        if (totalLeads == 0)
        {
            return await _bot.EditMessageText(
                chatId: action.ChatId,
                messageId: action.MessageId!.Value,
                text: "📭 این جستجو هیچ لیدی پیدا نکرده است.",
                replyMarkup: _keyboardProvider.GetProcessManagementKeyboard(process)
            );
        }

        // Prevent navigating past the last lead.
        if (index >= totalLeads)
            index = totalLeads - 1;

        var lead = await _dbContext.Leads
            .Where(l => l.ProcessId == processId)
            .OrderByDescending(l => l.Score)
            .ThenByDescending(l => l.MessageDate)
            .Skip(index)
            .FirstOrDefaultAsync();

        if (lead is null)
            throw new InvalidOperationException("Lead no longer exists.");

        var text =
            $"🎯 <b>لید {index + 1} از {totalLeads}</b>\n\n" +

            $"💬 <b>پیام:</b>\n" +
            $"<blockquote>{lead.Text}</blockquote>\n\n" +

            $"━━━━━━━━━━━━━━\n" +
            $"📊 امتیاز: {lead.Score}\n" +
            $"👤 شناسه کاربر: @{lead.Username}\n" +
            $"👥 گروه: {lead.GroupName}\n" +
            $"🕐 تاریخ: {FormatDate(lead.MessageDate)}";

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            messageId: action.MessageId!.Value,
            text: text,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            replyMarkup: _keyboardProvider.GetLeadNavigationKeyboard(
                processId,
                index,
                totalLeads,
                lead.GroupId,
                lead.TelegramMessageId)
        );
    }

    private static string FormatDate(Instant instant)
    {
        var zone = DateTimeZoneProviders.Tzdb["Asia/Tehran"];

        return instant
            .InZone(zone)
            .LocalDateTime
            .ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
    }
}
