using StackExchange.Redis;
using System.Text.Json;
using OppMonitor.Api.Pipeline;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Jobs;

public class RawDataReaderJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Pipeline.PipelineExecutor _pipelineExecutor;

    public RawDataReaderJob(IServiceProvider serviceProvider, PipelineExecutor pipeline)
    {
        _serviceProvider = serviceProvider;
        _pipelineExecutor = pipeline;
    }

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
            var processIdStr = result.GetProperty("scan_id").GetString();
            var processId = Guid.Parse(processIdStr!);

            using (var scope = _serviceProvider.CreateScope())
            {
                var configProvider = scope.ServiceProvider.GetRequiredService<ConfigProvider>();
                var config = await configProvider.GetConfigAsync(processId);

                if (config != null)
                {
                    await _pipelineExecutor.RunAsync(json.ToString(), config, stoppingToken);
                }
            }
        }
    }
}
