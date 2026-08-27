using Sarnakh.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Sarnakh.Api.Endpoints.Handlers;

public class GetLeadsHandler
{
    public static async Task<IResult> Handler(AppDbContext dbContext, Guid processId) //TODO just get it from pid for now
    {
        var leads = await dbContext.Leads
            .Where(l => l.ProcessId == processId)
            .ToListAsync();

        return Results.Ok(leads);
    }
}
