namespace BlazorWebApp.Classes;

public class TokenManagementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenManagementMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
    {
        _next = next;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken = context.Request.Cookies["MyCookieC2"];
        var refreshToken = context.Request.Cookies["MyCookieC1"];

        if (IsTokenExpired(accessToken))
        {
            var newTokens = await RefreshTokens(refreshToken);
            if (newTokens != null)
            {
                context.Response.Cookies.Append("access_token", newTokens.AccessToken);
                context.Response.Cookies.Append("refresh_token", newTokens.RefreshToken);
            }
            else
            {
                // Handle token refresh failure (e.g., redirect to login)
                context.Response.Redirect("/login");
                return;
            }
        }

        await _next(context);
    }

    private bool IsTokenExpired(string token)
    {
        // Logic to check if the token is expired
        return false; // Replace with actual expiration logic
    }

    private async Task<TokenResponse> RefreshTokens(string refreshToken)
    {
        // Logic to refresh tokens using Keycloak
        return new TokenResponse
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token"
        };
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}