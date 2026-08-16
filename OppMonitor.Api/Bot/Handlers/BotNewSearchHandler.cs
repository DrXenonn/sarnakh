using OppMonitor.Api.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using OppMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace OppMonitor.Api.Bot.Handlers;

public class BotNewSearchHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;

    public BotNewSearchHandler(
        TelegramBotClient bot,
        AppDbContext dbContext)
    {
        _bot = bot;
        _dbContext = dbContext;
    }

    public string Action => "new_search";

    public async Task<Message> HandleAsync(BotAction action)
    {
        await _dbContext.Processes
            .Where(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft)
            .ExecuteDeleteAsync();

        var newProcess = new Process
        {
            TelegramUserId = action.UserId,
            ProcessStatus = ProcessStatus.Draft,
            ProcessInputState = ProcessInputState.Name
        };

        await _dbContext.Processes.AddAsync(newProcess);
        await _dbContext.SaveChangesAsync();

        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [("⬅️ بازگشت به منوی اصلی", "/start")]
            ]
        };

        const string text =
            "🔎 جستجوی جدید\n\n" +
            "لطفاً یک نام برای جستجوی خود وارد کنید:";

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
}
