using OppMonitor.Api.Data;
using OppMonitor.Api.Models;

namespace OppMonitor.Api.Pipeline.Steps;

public class StoreCargoStep : IPipelineStep
{
    private readonly IServiceProvider _serviceProvider;

    public StoreCargoStep(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TelegramMessageContext?> ExecuteAsync(TelegramMessageContext ctx, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            TelegramMessageId = ctx.MessageId,
            SenderId = ctx.SenderId ?? 0,
            Username = ctx.Username,
            Text = ctx.Text,
            Score = ctx.Score,
            MessageDate = ctx.Date,
            GroupName = ctx.GroupName,
            GroupId = ctx.GroupId
        };

        db.Leads.Add(lead);
        await db.SaveChangesAsync(ct);

        return ctx;
    }
}
