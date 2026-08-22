using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers.ProcessCreation;

public class BotSelectGroupCategoriesHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotKeyboardProvider _keyboardProvider;
    private readonly ProcessManager _processManager;

    public BotSelectGroupCategoriesHandler(
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

    public string Action => "select_groups";

    public async Task<Message> HandleAsync(BotAction action)
    {
        Guid.TryParse(action.Data, out var categoryId);

        var category = await _dbContext.TelegramGroupCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        var draftProcess = await _processManager.GetDraftProcessAsync(action.UserId);

        if (draftProcess is not null && category is not null)
        {
            draftProcess.Config.TelegramChatIds = category.GroupIds;
            draftProcess.ProcessInputState = ProcessInputState.Keywords;
        }

        await _dbContext.SaveChangesAsync();

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            replyMarkup: await _keyboardProvider.GetKeywordPacksKeyboard(),
            messageId: action.MessageId!.Value,
            text:
                "🔑 لطفاً اولین کلمه کلیدی خود را وارد کنید.\n\n" +
                "مثال: ضد آفتاب"
        );
    }
}
