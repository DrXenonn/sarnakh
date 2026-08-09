using StackExchange.Redis;
using System.Text.Json;
using OppMonitor.Api.Pipeline;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Jobs;

public class RawDataReaderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabase _dbRedis;

    public RawDataReaderJob(IServiceProvider serviceProvider, IConnectionMultiplexer redis)
    {
        _dbRedis = redis.GetDatabase();
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var json = await _dbRedis.ListLeftPopAsync("telegram_results");

            if (json.IsNull)
            {
                await Task.Delay(2000, stoppingToken);
                continue;
            }

            var result = JsonSerializer.Deserialize<JsonElement>(json.ToString());
            var processIdStr = result.GetProperty("process_id").GetString();
            var processId = Guid.Parse(processIdStr!);

            using (var scope = _serviceProvider.CreateScope())
            {
                var configProvider = scope.ServiceProvider.GetRequiredService<ConfigProvider>();
                var config = await configProvider.GetConfigAsync(processId);

                if (config != null)
                {
                    var pipelineExecutor = scope.ServiceProvider.GetRequiredService<PipelineExecutor>();
                    await pipelineExecutor.RunAsync(json.ToString(), config, processId, stoppingToken);
                }
            }
        }
    }
}
