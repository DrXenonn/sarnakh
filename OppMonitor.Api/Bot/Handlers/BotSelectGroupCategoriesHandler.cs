using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotSelectGroupCategoriesHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;

    public BotSelectGroupCategoriesHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotKeyboardProvider keyboardProvider)
    {
        _bot = bot;
        _dbContext = dbContext;
        _keyboardProvider = keyboardProvider;
    }

    public string Action => "select_groups";

    public async Task<Message> HandleAsync(BotAction action)
    {
        if (action.Data is null || !Guid.TryParse(action.Data, out var categoryId))
            throw new InvalidOperationException("Invalid group category ID.");

        var category = await _dbContext.TelegramGroupCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is null)
            throw new InvalidOperationException("Group category not found.");

        var draftProcess = await _dbContext.Processes
            .Include(p => p.Config)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == action.UserId &&
                p.ProcessStatus == ProcessStatus.Draft);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        draftProcess.Config.TelegramChatIds = category.GroupIds;
        draftProcess.ProcessInputState = ProcessInputState.Keywords;

        await _dbContext.SaveChangesAsync();

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            replyMarkup: await _keyboardProvider.GetKeywordPacksKeyboard(_dbContext),
            messageId: action.MessageId!.Value,
            text:
                "🔑 لطفاً اولین کلمه کلیدی خود را وارد کنید.\n\n" +
                "مثال: ضد آفتاب"
        );
    }
}
