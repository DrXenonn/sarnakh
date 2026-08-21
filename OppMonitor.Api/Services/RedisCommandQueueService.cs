using System.Text.Json;
using StackExchange.Redis;

namespace OppMonitor.Api.Services;

public class RedisCommandQueueService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisCommandQueueService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task QueueScanHistoricalAsync(
        Guid processId,
        IEnumerable<long> groupIds,
        int daysBack)
    {
        var command = new
        {
            action = "scan_historical",
            process_id = processId.ToString(),
            group_ids = groupIds,
            days_back = daysBack
        };

        var db = _redis.GetDatabase();

        await db.ListRightPushAsync(
            "telegram_commands",
            JsonSerializer.Serialize(command)
        );
    }
}
