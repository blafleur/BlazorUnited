using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BlazorWebApp.Classes;

public class HttpContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
    }

    public async Task<string> GetIdToken()
    {
        var idToken =  await _httpContextAccessor.HttpContext?.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
       
        return idToken ?? "Empty";
    }

    public async Task<string> GetAccessToken()
    {
        var accessToken = await _httpContextAccessor.HttpContext?.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
        return accessToken ?? "Empty";
    }

    public async Task<string> GetRefreshToken()
    {
        var refreshToken = await _httpContextAccessor.HttpContext?.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);
        return refreshToken ?? "Empty"; 
    }
}