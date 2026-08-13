using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class BotMainmenuHandler : IBotHandler
{
    private readonly TelegramBotClient _bot;

    public BotMainmenuHandler(TelegramBotClient bot)
    {
        _bot = bot;
    }

    public string Action => "/start";

    public async Task<Message> HandleAsync(BotAction action)
    {
        Console.WriteLine("yo");
        return await _bot.SendMessage(action.ChatId, "Mainmenu");
    }
}
