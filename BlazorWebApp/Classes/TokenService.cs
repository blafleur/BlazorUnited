using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlazorWebApp.Classes;

public class TokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<string> RefreshAccessTokenAsync()
    {
        var refreshToken = _httpContextAccessor.HttpContext.User.FindFirst("refresh_token")?.Value;
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new InvalidOperationException("No refresh token available.");
        }

        var client = new HttpClient();
        var discoveryDocument = await client.GetDiscoveryDocumentAsync(_configuration["OIDC:Authority"]);
        if (discoveryDocument.IsError)
        {
            throw new InvalidOperationException("Failed to retrieve discovery document.");
        }

        var tokenResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address = discoveryDocument.TokenEndpoint,
            ClientId = _configuration["OIDC:ClientId"],
            ClientSecret = _configuration["OIDC:ClientSecret"],
            RefreshToken = refreshToken
        });

        if (tokenResponse.IsError)
        {
            throw new InvalidOperationException("Failed to refresh token.");
        }

        // Update the authentication properties with the new tokens
        var authProperties = new AuthenticationProperties();
        authProperties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = tokenResponse.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = tokenResponse.RefreshToken }
        });

        await _httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            _httpContextAccessor.HttpContext.User,
            authProperties);

        return tokenResponse.AccessToken;
    }
}