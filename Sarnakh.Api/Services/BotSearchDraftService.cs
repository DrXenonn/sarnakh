// using Microsoft.EntityFrameworkCore;
// using Sarnakh.Api.Data;
// using Sarnakh.Api.Models;
//
// namespace Sarnakh.Api.Services;
//
// public class BotSearchDraftService
// {
//     private readonly AppDbContext _dbContext;
//
//     public BotSearchDraftService(AppDbContext dbContext)
//     {
//         _dbContext = dbContext;
//     }
//
//     public async Task<Process?> GetDraftProcessAsync(long userId)
//     {
//         return await _dbContext.Processes
//             .Include(p => p.Config)
//             .ThenInclude(c => c.Keywords)
//             .FirstOrDefaultAsync(p =>
//                 p.TelegramUserId == userId &&
//                 p.ProcessStatus == ProcessStatus.Draft);
//     }
// }
