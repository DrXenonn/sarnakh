using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMessageHandler
{
    private readonly AppDbContext _dbContext;
    private readonly TelegramBotClient _bot;
    private readonly BotActionHandlerService _actionHandler;

    public BotMessageHandler(
        AppDbContext dbContext,
        TelegramBotClient bot,
        BotActionHandlerService actionHandler)
    {
        _dbContext = dbContext;
        _bot = bot;
        _actionHandler = actionHandler;
    }

    public async Task HandleAsync(Message message)
    {

    }
}
