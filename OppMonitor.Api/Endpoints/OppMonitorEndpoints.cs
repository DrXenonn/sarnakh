using System.Security.Claims;
using OppMonitor.Api.Endpoints.Handlers;

namespace OppMonitor.Api.Endpoints;

public static class StarterEndpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/app");

        group.MapPost("/register", RegisterHandler.Handler);
        group.MapPost("/login", LoginHandler.Handler);
        group.MapPost("/refresh-token", RefreshTokenHandler.Handler);
        group.MapPost("/logout", LogoutHandler.Handler);
        group.MapGet("/me", (ClaimsPrincipal claimsPrincipal) =>
                {
                    return Results.Ok(claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value));
                }).RequireAuthorization();
    }
}
