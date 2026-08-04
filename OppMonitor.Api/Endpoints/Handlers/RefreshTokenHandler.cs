using Microsoft.EntityFrameworkCore;
using NodaTime;
using OppMonitor.Api.Data;
using OppMonitor.Api.Extensions;
using OppMonitor.Api.Models;
using OppMonitor.Api.Services;

namespace OppMonitor.Api.Endpoints.Handlers;

public class RefreshTokenHandler
{
    public static async Task<IResult> Handler(
        AppDbContext dbContext,
        TokenProvider tokenProvider,
        HttpContext httpContext)
    {
        var refreshToken = httpContext.Request.Cookies["refreshToken"];

        RefreshToken? storedRefreshToken = await dbContext.RefreshTokens
            .Include(rt => rt.ApplicationUser)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedRefreshToken is null || storedRefreshToken.Expires < SystemClock.Instance.GetCurrentInstant())
        {
            return Results.Unauthorized();
        }

        var accessToken = await tokenProvider.CreateAccessToken(storedRefreshToken.ApplicationUser);

        storedRefreshToken.Token = tokenProvider.GenerateRefreshToken();
        storedRefreshToken.Expires = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(7);

        httpContext.AppendAccessTokenCookie(accessToken);
        httpContext.AppendRefreshTokenCookie(storedRefreshToken.Token);

        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }
}
