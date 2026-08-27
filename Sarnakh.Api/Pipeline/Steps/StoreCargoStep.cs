using Sarnakh.Api.Data;
using Sarnakh.Api.Models;

namespace Sarnakh.Api.Pipeline.Steps;

public class StoreCargoStep : IPipelineStep
{
    private readonly AppDbContext _dbContext;

    public StoreCargoStep(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TelegramMessageContext?> ExecuteAsync(TelegramMessageContext ctx, CancellationToken ct)
    {
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

        lead.ProcessId = ctx.Process.Id;
        _dbContext.Leads.Add(lead);

        await _dbContext.SaveChangesAsync(ct);
        return ctx;
    }
}
