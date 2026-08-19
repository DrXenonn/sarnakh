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
}
