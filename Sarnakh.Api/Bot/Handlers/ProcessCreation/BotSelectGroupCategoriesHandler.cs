using Microsoft.EntityFrameworkCore;
using Sarnakh.Api.Data;
using Sarnakh.Api.Models;
using Sarnakh.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sarnakh.Api.Bot.Handlers.ProcessCreation;

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

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        await _dbContext.SaveChangesAsync();

        return await _bot.EditMessageText(
            chatId: action.ChatId,
            replyMarkup: await _keyboardProvider.GetKeywordPacksKeyboard(draftProcess),
            messageId: action.MessageId!.Value,
            text:
            "🔑 کلمات کلیدی\n\n" +
            "کلمات کلیدی مشخص می‌کنند سرنخ به دنبال چه کلمه یا عبارتی در گفتگوها بگردد.\n\n" +
            "📦 می‌توانید یکی از بسته‌های آماده زیر را انتخاب کنید؛ " +
            "این بسته‌ها شامل کلمات کلیدی مرتبط و از پیش تنظیم‌شده هستند.\n\n" +
            "✏️ یا یک کلمه کلیدی دلخواه را همین‌جا در چت ارسال کنید.\n\n" +
            "مثال: ضد آفتاب");
    }
}
