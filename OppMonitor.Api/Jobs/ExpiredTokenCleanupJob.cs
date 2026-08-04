using Microsoft.EntityFrameworkCore;
using NodaTime;
using OppMonitor.Api.Data;

namespace OppMonitor.Api.Jobs;

public class ExpiredTokenCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _timer;

    public ExpiredTokenCleanupJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _timer = new PeriodicTimer(TimeSpan.FromHours(1));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.RefreshTokens
                .Where(rt => rt.Expires < SystemClock.Instance.GetCurrentInstant())
                .ExecuteDeleteAsync(stoppingToken);
        }
    }
}
