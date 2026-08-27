using Microsoft.AspNetCore.Identity;
using NodaTime;
using Sarnakh.Api.Data;
using Sarnakh.Api.Dtos;
using Sarnakh.Api.Extensions;
using Sarnakh.Api.Models;
using Sarnakh.Api.Services;

namespace Sarnakh.Api.Endpoints.Handlers;

public class LoginHandler
{
    public static async Task<IResult> Handler(
        LoginDto loginDto,
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        TokenProvider tokenProvider,
        HttpContext httpContext)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user is null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return Results.Unauthorized();
        }
        string accessToken = await tokenProvider.CreateAccessToken(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            Expires = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(7)
        };

        httpContext.AppendAccessTokenCookie(accessToken);
        httpContext.AppendRefreshTokenCookie(refreshToken.Token);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }
}
