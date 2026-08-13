using Telegram.Bot;
using Telegram.Bot.Types;

namespace OppMonitor.Api.Bot.Handlers;

public class SayHelloHandler : IBotCallbackHandler
{
    private readonly TelegramBotClient _bot;

    public SayHelloHandler(TelegramBotClient bot)
    {
        _bot = bot;
    }

    public string Action => "say_hello";

    public async Task HandleAsync(CallbackQuery callbackQuery)
    {
        Console.WriteLine("yo");
        await _bot.SendMessage(callbackQuery.Message!.Chat, "yo");
    }
}
