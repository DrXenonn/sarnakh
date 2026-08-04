namespace OppMonitor.Api.Extensions;

public static class HttpContextExtensions
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    public static void AppendAccessTokenCookie(this HttpContext httpContext, string token)
    {
        httpContext.Response.Cookies.Append(AccessTokenCookieName, token, new CookieOptions
        {
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(2)
        });
    }

    public static void AppendRefreshTokenCookie(this HttpContext httpContext, string token)
    {
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public static void DeleteAuthCookies(this HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(AccessTokenCookieName);
        httpContext.Response.Cookies.Delete(RefreshTokenCookieName);
    }

    public static string? GetRefreshToken(this HttpContext httpContext)
    {
        return httpContext.Request.Cookies["refreshToken"];
    }
}
