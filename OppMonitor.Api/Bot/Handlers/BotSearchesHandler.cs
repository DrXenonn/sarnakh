using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OppMonitor.Api.Bot.Handlers;

public class BotSearchesHandler : IBotHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;

    public BotSearchesHandler(
        TelegramBotClient bot,
        AppDbContext dbContext)
    {
        _bot = bot;
        _dbContext = dbContext;
    }

    public string Action => "searches";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var processes = await GetUserProcesses(action.UserId);

        var buttons = processes
            .Select(process => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    GetProcessStatusText(process.ProcessStatus),
                    $"search:{process.Id}"
                )
            })
            .ToList();

        // Add the back button at the end
        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
                "🔙 بازگشت",
                "/start"
            )
        ]);

        var keyboard = new InlineKeyboardMarkup(buttons);

        var text = processes.Count == 0
            ? "📋 هنوز هیچ جستجویی ایجاد نکرده‌اید."
            : "📋 جستجوهای من:";

        if (action.MessageId is not null)
        {
            return await _bot.EditMessageText(
                chatId: action.ChatId,
                messageId: action.MessageId.Value,
                text: text,
                replyMarkup: keyboard
            );
        }

        return await _bot.SendMessage(
            chatId: action.ChatId,
            text: text,
            replyMarkup: keyboard
        );
    }

    private async Task<List<Process>> GetUserProcesses(long userId)
    {
        return await _dbContext.Processes
            .Where(p => p.TelegramUserId == userId)
            .ToListAsync();
    }

    private static string GetProcessStatusText(ProcessStatus status)
    {
        return status switch
        {
            ProcessStatus.Pending => "🟡 در انتظار",
            ProcessStatus.Running => "🟢 در حال جستجو",
            ProcessStatus.Completed => "✅ تکمیل‌شده",
            ProcessStatus.Stopped => "⏹ متوقف‌شده",
            ProcessStatus.Failed => "❌ ناموفق",
            _ => "❓ نامشخص"
        };
    }
}
