using OppMonitor.Api.Data;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;

namespace OppMonitor.Api.Services;

public class BotKeyboardProvider
{
    public async Task<InlineKeyboardMarkup> GetKeywordPacksKeyboard(AppDbContext dbContext)
    {
        var keywordPacks = await dbContext.KeywordPacks.ToListAsync();

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
}
