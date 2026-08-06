using StackExchange.Redis;
using System.Text.Json;

namespace OppMonitor.Api.Jobs;

public class RawDataReaderJob : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        var dbRedis = redis.GetDatabase();

        while (!stoppingToken.IsCancellationRequested)
        {
            var json = dbRedis.ListLeftPop("telegram_results");

            if (json.IsNull)
            {
                await Task.Delay(2000, stoppingToken);
                continue;
            }

            var result = JsonSerializer.Deserialize<JsonElement>(json.ToString());
            var messages = result.GetProperty("messages");

            foreach (var message in messages.EnumerateArray())
            {
                Console.WriteLine(message);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
