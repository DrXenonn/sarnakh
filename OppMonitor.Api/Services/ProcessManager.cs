using OppMonitor.Api.Data;
using OppMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace OppMonitor.Api.Services;

public class ProcessManager
{
    private readonly AppDbContext _dbContext;

    public ProcessManager(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Process>> GetProcessesForTelegramUserAsync(long userId)
    {
        return await _dbContext.Processes
            .Include(p => p.Config)
            .ThenInclude(c => c.Keywords)
            .Where(p =>
                p.TelegramUserId == userId)
            .ToListAsync();
    }

    public async Task<Process?> GetProcessAsync(Guid processId)
    {
        return await _dbContext.Processes
            .Include(p => p.Config)
            .ThenInclude(c => c.Keywords)
            .FirstOrDefaultAsync(p => p.Id == processId);
    }

    public async Task<Process?> GetDraftProcessAsync(long userId)
    {
        return await _dbContext.Processes
            .Include(p => p.Config)
            .ThenInclude(c => c.Keywords)
            .FirstOrDefaultAsync(p =>
                p.TelegramUserId == userId &&
                p.ProcessStatus == ProcessStatus.Draft);
    }

    public async Task DeleteProcessAsync(Guid processId, long userId)
    {
        var process = await _dbContext.Processes
           .FirstOrDefaultAsync(p =>
               p.Id == processId &&
               p.TelegramUserId == userId);

        if (process is not null)
        {
            _dbContext.Processes.Remove(process);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateStatus(Process process, string? statusJson)
    {
        var newStatus = statusJson switch
        {
            "running" => ProcessStatus.Running,
            "stopped" => ProcessStatus.Stopped,
            "completed" => ProcessStatus.Completed,
            _ => ProcessStatus.Failed
        };

        if (process.ProcessStatus == newStatus)
            return;

        process.ProcessStatus = newStatus;
        await _dbContext.SaveChangesAsync();
    }

    public string GetStatusText(ProcessStatus status)
    {
        return status switch
        {
            ProcessStatus.Draft => "📝 پیش‌نویس",
            ProcessStatus.Running => "📡 در حال بررسی",
            ProcessStatus.Stopped => "⏹️ متوقف شده",
            ProcessStatus.Completed => "✅ تکمیل شده",
            ProcessStatus.Failed => "❌ ناموفق",
            ProcessStatus.Pending => "⏳ در صف اجرا",
            _ => "❓ نامشخص"
        };
    }
}
