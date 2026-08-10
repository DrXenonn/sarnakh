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
            var processStatus = result.GetProperty("status").GetString();
            var processId = Guid.Parse(processIdStr!);

            using (var scope = _serviceProvider.CreateScope())
            {
                var processProvider = scope.ServiceProvider.GetRequiredService<ProcessManager>();
                var process = await processProvider.GetProcessAsync(processId);

                if (process != null)
                {
                    await processProvider.UpdateStatus(process, processStatus);
                    var pipelineExecutor = scope.ServiceProvider.GetRequiredService<PipelineExecutor>();
                    await pipelineExecutor.RunAsync(json.ToString(), process, stoppingToken);
                }
            }
        }
    }
}
