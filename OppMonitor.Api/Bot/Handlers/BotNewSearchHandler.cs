using OppMonitor.Api.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using OppMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Bot.Handlers;

public class BotNewSearchHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;

    public BotNewSearchHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
    }
    public string Action => "new_search";

    public async Task<Message> HandleAsync(BotAction action)
    {
        await _dbContext.Processes
            .Where(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft)
            .ExecuteDeleteAsync();

        var config = new Config
        {
            Id = Guid.NewGuid()
        };

        var newProcess = new Process
        {
            Id = Guid.NewGuid(),
            TelegramUserId = action.UserId,
            ProcessStatus = ProcessStatus.Draft,
            ProcessInputState = ProcessInputState.Name,
            ConfigId = config.Id,
            Config = config
        };

        _dbContext.Configs.Add(config);
        _dbContext.Processes.Add(newProcess);

        await _dbContext.SaveChangesAsync();

        var keyboard = _keyboardProvider.GetBackToMainMenuKeyboard();

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
