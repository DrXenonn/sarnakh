using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotSelectSearchDaysHandler : IBotActionHandler
{
    private readonly TelegramBotClient _bot;
    private readonly AppDbContext _dbContext;
    private readonly BotSearchDraftService _draftService;

    public BotSelectSearchDaysHandler(
        TelegramBotClient bot,
        AppDbContext dbContext,
        BotSearchDraftService draftService)
    {
        _bot = bot;
        _dbContext = dbContext;
        _draftService = draftService;
    }

    public string Action => "select_search_days";

    public async Task<Message> HandleAsync(BotAction action)
    {
        var draftProcess = await _draftService.GetDraftProcessAsync(action.UserId);

        if (draftProcess is null)
            throw new InvalidOperationException("Draft process not found.");

        draftProcess.ProcessInputState = ProcessInputState.SearchDays;

        await _dbContext.SaveChangesAsync();

        return await _bot.SendMessage(
            chatId: action.ChatId,
            text:
                "⏳ چند روز گذشته را بررسی کنیم؟\n\n" +
                "مثال:\n" +
                "7 یعنی بررسی ۷ روز اخیر\n" +
                "30 یعنی بررسی ۳۰ روز اخیر"
        );
    }
}
