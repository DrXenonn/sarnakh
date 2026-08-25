using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMainmenuHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly AppDbContext _dbContext;

    public BotMainmenuHandler(
        TelegramBotClient bot,
        BotKeyboardProvider keyboardProvider,
        AppDbContext dbContext)
    {
        _bot = bot;
        _keyboardProvider = keyboardProvider;
        _dbContext = dbContext;
    }

    public string Action => "start";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var userProcessIds = _dbContext.Processes
            .Where(p => p.TelegramUserId == action.UserId)
            .Select(p => p.Id);

        var searchCount = await userProcessIds
            .CountAsync();

        var leadsCount = await _dbContext.Leads
            .CountAsync(l => userProcessIds.Contains(l.ProcessId));

        var keyboard = _keyboardProvider.GetMainMenuKeyboard();

        var text =
            "🔎 <b>سرنخ</b>\n\n" +
            "سرنخ‌های جدید را از گفتگوهای مرتبط در تلگرام پیدا کنید.\n\n" +
            $"📂 <b>جستجوهای شما:</b> {searchCount}\n" +
            $"🎯 <b>سرنخ‌های پیدا شده:</b> {leadsCount}\n\n" +
            "چه کاری می‌خواهید انجام دهید؟";

        if (action.MessageId is not null)
        {
            return await _bot.EditMessageText(
                chatId: action.ChatId,
                messageId: action.MessageId.Value,
                text: text,
                replyMarkup: keyboard,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );
        }

        return await _bot.SendMessage(
            chatId: action.ChatId,
            text: text,
            replyMarkup: keyboard,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
        );
    }
}
