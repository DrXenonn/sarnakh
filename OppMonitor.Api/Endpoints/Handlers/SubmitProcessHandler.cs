using Microsoft.AspNetCore.Identity;
using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using StackExchange.Redis;
using System.Text.Json;
using OppMonitor.Api.Dtos;

namespace OppMonitor.Api.Endpoints.Handlers;

public class SubmitProcessHandler
{
    public static async Task<IResult> Handler(
            AppDbContext dbContext,
            IConnectionMultiplexer redis,
            UserManager<ApplicationUser> UserManager,
            ProcessConfigDto configDto)
    {
        var newConfig = new Config
        {
            Id = Guid.NewGuid(),
            Keywords = configDto.Keywords,
            TelegramChatIds = configDto.TelegramChatIds,
            SearchDays = configDto.SearchDays,
            ScoreThreshold = configDto.ScoreThreshold
        };

        var newProcess = new Process
        {
            Id = Guid.NewGuid(),
            ProcessStatus = ProcessStatus.Pending,
            ConfigId = newConfig.Id
        };
        await dbContext.Configs.AddAsync(newConfig);
        await dbContext.Processes.AddAsync(newProcess);
        await dbContext.SaveChangesAsync();

        var command = new
        {
            action = "scan_historical",
            process_id = newProcess.Id.ToString(),
            group_ids = configDto.TelegramChatIds,
            days_back = configDto.SearchDays
        };

        var dbRedis = redis.GetDatabase();
        await dbRedis.ListRightPushAsync("telegram_commands", JsonSerializer.Serialize(command));

        return Results.Ok(new { ProcessId = newProcess.Id });
    }
}
