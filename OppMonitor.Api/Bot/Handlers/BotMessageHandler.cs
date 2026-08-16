using Microsoft.EntityFrameworkCore;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMessageHandler
{
    private readonly AppDbContext _dbContext;
    private readonly TelegramBotClient _bot;

    public BotMessageHandler(
        AppDbContext dbContext,
        TelegramBotClient bot)
    {
        _dbContext = dbContext;
        _bot = bot;
    }

    public async Task HandleAsync(Message message)
    {
    }
}
