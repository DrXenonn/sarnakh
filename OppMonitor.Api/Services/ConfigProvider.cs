using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace OppMonitor.Api.Services;

public class ConfigProvider
{
    private readonly AppDbContext _dbContext;

    public ConfigProvider(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Config?> GetConfigAsync(Guid processId)
    {
        var process = await _dbContext.Processes
            .FirstOrDefaultAsync(p => p.Id == processId);

        return process?.Config;
    }
}
