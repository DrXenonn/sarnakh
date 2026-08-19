using OppMonitor.Api.Data;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;

namespace OppMonitor.Api.Services;

public class BotKeyboardProvider
{
    private readonly AppDbContext _dbContext;

    public BotKeyboardProvider(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InlineKeyboardMarkup> GetKeywordPacksKeyboard()
    {
        var keywordPacks = await _dbContext.KeywordPacks.ToListAsync();

        var keyboardButtons = new List<IEnumerable<InlineKeyboardButton>>();

        foreach (var keywrodpack in keywordPacks)
        {
            var button = InlineKeyboardButton.WithCallbackData
                    ($"{keywrodpack.Name}", $"add_keyword_pack:{keywrodpack.Id}");

            keyboardButtons.Add([button]);
        }

        var keyboard = new InlineKeyboardMarkup
        {
            InlineKeyboard = keyboardButtons
        };

        return keyboard;
    }

    public async Task<InlineKeyboardMarkup> GetKeywordPacksWithDurationPick()
    {
        var keywordPacksKeyboard = await GetKeywordPacksKeyboard();

        var buttons = keywordPacksKeyboard.InlineKeyboard.ToList();

        buttons.Add(
            [InlineKeyboardButton.WithCallbackData(
            "📅 انتخاب تعداد روز",
            "select_search_days")]
        );

        return new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
        };
    }

    public async Task<InlineKeyboardMarkup> GetGroupCategoriesKeyboard()
    {
        var groupCategories = await _dbContext.TelegramGroupCategories
            .ToListAsync();

        var buttons = new List<IEnumerable<InlineKeyboardButton>>();

        foreach (var groupCategory in groupCategories)
        {
            var button = InlineKeyboardButton.WithCallbackData(
                $"{groupCategory.Name} ({groupCategory.GroupIds.Count} گروه)",
                $"select_groups:{groupCategory.Id}");

            buttons.Add([button]);
        }

        return new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
        };
    }

    public InlineKeyboardMarkup GetBackToMainMenuKeyboard()
    {
        return new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [("⬅️ بازگشت به منوی اصلی", "/start")]
            ]
        };
    }

    public InlineKeyboardMarkup GetMainMenuKeyboard()
    {
        return new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [("🔎 جستجوی جدید", "new_search")],
            [("📋 جستجوهای من", "searches")]
            ]
        };
    }

    public InlineKeyboardMarkup GetKeywordWeightKeyboard(Guid keywordId)
    {
        return new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                InlineKeyboardButton.WithCallbackData("1", $"keyword_weight:1:{keywordId}"),
                InlineKeyboardButton.WithCallbackData("2", $"keyword_weight:2:{keywordId}"),
                InlineKeyboardButton.WithCallbackData("3", $"keyword_weight:3:{keywordId}"),
                InlineKeyboardButton.WithCallbackData("4", $"keyword_weight:4:{keywordId}"),
                InlineKeyboardButton.WithCallbackData("5", $"keyword_weight:5:{keywordId}")
            ]
            ]
        };
    }
}
