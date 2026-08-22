using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using Telegram.Bot.Types.ReplyMarkups;

namespace OppMonitor.Api.Services;

public class BotKeyboardProvider
{
    private readonly AppDbContext _dbContext;
    private readonly ProcessManager _processManager;

    public BotKeyboardProvider(AppDbContext dbContext, ProcessManager processManager)
    {
        _dbContext = dbContext;
        _processManager = processManager;
    }

    public async Task<InlineKeyboardMarkup> GetKeywordPacksKeyboard()
    {
        var keywordPacks = await _dbContext.KeywordPacks
            .Include(k => k.Keywords)
            .ToListAsync();

        var buttons = new List<IEnumerable<InlineKeyboardButton>>();

        foreach (var keywordPack in keywordPacks)
        {
            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                    $"{keywordPack.Name} ({keywordPack.Keywords.Count} کلمه)",
                    $"add_keyword_pack:{keywordPack.Id}")
            ]);
        }

        return new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
        };
    }

    public async Task<InlineKeyboardMarkup> GetKeywordSelectionKeyboard()
    {
        var keywordPacksKeyboard = await GetKeywordPacksKeyboard();

        var buttons = keywordPacksKeyboard.InlineKeyboard.ToList();

        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
                "✅ مرحله بعدی (انتخاب حد امتیاز)",
                "select_score_threshold")
        ]);

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
            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                    $"{groupCategory.Name} ({groupCategory.GroupIds.Count} گروه)",
                    $"select_groups:{groupCategory.Id}")
            ]);
        }

        return new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
        };
    }

    public async Task<InlineKeyboardMarkup> GetProcessesKeyboard(long userId)
    {
        var processes = await _processManager.GetProcessesForTelegramUserAsync(userId);

        var buttons = new List<IEnumerable<InlineKeyboardButton>>();

        foreach (var process in processes)
        {
            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                $"{process.Name} ({_processManager.GetStatusText(process.ProcessStatus)})",
                $"manage_process:{process.Id}")
            ]);
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
                [
                    InlineKeyboardButton.WithCallbackData(
                        "⬅️ بازگشت به منوی اصلی",
                        "start")
                ]
            ]
        };
    }

    public InlineKeyboardMarkup GetMainMenuKeyboard()
    {
        return new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        "🔎 جستجوی جدید",
                        "new_search")
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        "📋 جستجوهای من",
                        "show_searches")
                ]
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
                    InlineKeyboardButton.WithCallbackData(
                        "1",
                        $"keyword_weight:1:{keywordId}"),

                    InlineKeyboardButton.WithCallbackData(
                        "2",
                        $"keyword_weight:2:{keywordId}"),

                    InlineKeyboardButton.WithCallbackData(
                        "3",
                        $"keyword_weight:3:{keywordId}"),

                    InlineKeyboardButton.WithCallbackData(
                        "4",
                        $"keyword_weight:4:{keywordId}"),

                    InlineKeyboardButton.WithCallbackData(
                        "5",
                        $"keyword_weight:5:{keywordId}")
                ]
            ]
        };
    }

    public InlineKeyboardMarkup GetSummaryKeyboard()
    {
        return new InlineKeyboardMarkup
        {
            InlineKeyboard =
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        "🚀 شروع جستجو",
                        "start_process")
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        " انصراف و بازگشت",
                        "start")
                ]
            ]
        };
    }
}
