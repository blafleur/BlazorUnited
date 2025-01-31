using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BlazorWebApp.Classes;

public class LogoutMiddleware
{
    private readonly RequestDelegate _next;
    

    public LogoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, HttpContextService service)
    {
        if (context.Request.Path.Equals("/logout", StringComparison.OrdinalIgnoreCase))
        {
            if (context.Request.Headers["Blazor-Render-Mode"] == "InteractiveServer")
            {
                Console.WriteLine("Interactiveserver");
            }

            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            var id = await service.GetRefreshToken();
            var url = $"http://localhost:8080/realms/aspnet/protocol/openid-connect/logout";
           // var url = $"http://localhost:8080/realms/aspnet/protocol/openid-connect/logout?id_token_hint={id}&post_logout_redirect_uri=https://localhost:7093/";
            context.Response.Redirect(url);
            
            return;
        }
        
        await _next(context);
    }
}