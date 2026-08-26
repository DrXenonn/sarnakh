using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
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

    public async Task<InlineKeyboardMarkup> GetKeywordPacksKeyboard(Process process)
    {
        var keywordPacks = await _dbContext.KeywordPacks
            .Where(k => !process.Config.KeywordPackIds.Contains(k.Id))
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

    public async Task<InlineKeyboardMarkup> GetKeywordSelectionKeyboard(Process process)
    {
        var keywordPacksKeyboard = await GetKeywordPacksKeyboard(process);

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
                    $"select_groups:{groupCategory.Id}"),
            ]);
        }

        buttons.Add([
                InlineKeyboardButton.WithCallbackData(
                "⬅️ بازگشت به منوی اصلی",
                "start")]);

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
            if (process.ProcessStatus == ProcessStatus.Draft)
                continue;

            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                $"{process.Name} ({_processManager.GetStatusText(process.ProcessStatus)})",
                $"manage_search:{process.Id}")
            ]);
        }

        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
            "⬅️ بازگشت به منوی اصلی",
            "start"),
            InlineKeyboardButton.WithCallbackData(
                "🔄 بروزرسانی",
                "show_searches")
        ]);

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
                    "🟢 کم",
                    $"keyword_weight:1:{keywordId}"),

                InlineKeyboardButton.WithCallbackData(
                    "🟡 متوسط",
                    $"keyword_weight:2:{keywordId}"),

                InlineKeyboardButton.WithCallbackData(
                    "🔴 زیاد",
                    $"keyword_weight:3:{keywordId}")
            ]
            ]
        };
    }

    public InlineKeyboardMarkup GetProcessManagementKeyboard(Process process)
    {
        var buttons = new List<IEnumerable<InlineKeyboardButton>>();
        var status = process.ProcessStatus;
        var processId = process.Id;

        if (status == ProcessStatus.Completed)
        {
            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                "👀 مشاهده سرنخ‌ها",
                $"show_leads:{processId}:0")
            ]);
        }

        // if (status == ProcessStatus.Running)
        // {
        //     buttons.Add(
        //     [
        //         InlineKeyboardButton.WithCallbackData(
        //         "⏹️ توقف جستجو",
        //         $"stop_search:{processId}")
        //     ]);
        // }

        if (status is ProcessStatus.Completed or ProcessStatus.Stopped or ProcessStatus.Failed)
        {
            buttons.Add(
            [
                InlineKeyboardButton.WithCallbackData(
                "🔄 اجرای مجدد",
                $"rerun_search:{processId}")
            ]);
            buttons.Add(
                    [
                    InlineKeyboardButton.WithCallbackData(
                        "🗑️ حذف جستجو",
                        $"delete_search:{processId}")
                    ]);
        }

        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
            "⬅️ بازگشت به جستجوهای من",
            "show_searches")
        ]);

        return new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
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

    public InlineKeyboardMarkup GetLeadNavigationKeyboard(
        Guid processId,
        int page,
        int totalLeads,
        long chatId,
        int messageId)
    {
        var buttons = new List<IEnumerable<InlineKeyboardButton>>();

        var navigationButtons = new List<InlineKeyboardButton>();

        if (page > 0)
        {
            navigationButtons.Add(
                InlineKeyboardButton.WithCallbackData(
                    "⬅️ قبلی",
                    $"show_leads:{processId}:{page - 1}"
                )
            );
        }

        if (page < totalLeads - 1)
        {
            navigationButtons.Add(
                InlineKeyboardButton.WithCallbackData(
                    "بعدی ➡️",
                    $"show_leads:{processId}:{page + 1}"
                )
            );
        }

        if (navigationButtons.Count > 0)
            buttons.Add(navigationButtons);

        buttons.Add(
        [
            InlineKeyboardButton.WithUrl(
            "🔗 مشاهده پیام",
            BuildTelegramMessageLink(chatId, messageId)
        )
        ]);

        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData(
            "🔙 مدیریت جستجو",
            $"manage_search:{processId}"
        )
        ]);

        return new InlineKeyboardMarkup
        {
            InlineKeyboard = buttons
        };
    }

    private static string BuildTelegramMessageLink(
        long chatId,
        long messageId)
    {
        var chatIdString = chatId.ToString();

        if (chatIdString.StartsWith("-100"))
            chatIdString = chatIdString[4..];

        return $"https://t.me/c/{chatIdString}/{messageId}";
    }
}
