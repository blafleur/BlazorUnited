using System.IdentityModel.Tokens.Jwt;
using BlazorWebApp.Classes;
using BlazorWebApp.Components;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddCircuitOptions(
        options => { options.DetailedErrors = true; }
    );
builder.Services.AddCascadingAuthenticationState();

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<HttpContextService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
/*
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    }).AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.ClientId = builder.Configuration["Keycloak:ClientId"];
        options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.Scope.Add("openid");
        options.CallbackPath = "/signin-oidc";
        options.RequireHttpsMetadata = false; // for dev with no ssl
        options.SaveTokens = true;
        options.UsePkce = true;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";
    });
*/
const string MS_OIDC_SCHEME = "MicrosoftOidc";

builder.Services.AddAuthentication(
        o =>
        {
            o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        }
    ).AddOpenIdConnect(
        oidcOptions =>
        {
            oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            oidcOptions.Authority = builder.Configuration["Keycloak:Authority"];
            oidcOptions.ClientId = builder.Configuration["Keycloak:ClientId"];
            oidcOptions.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];

            oidcOptions.UsePkce = true;
            oidcOptions.Scope.Add(OpenIdConnectScope.OpenId);
            oidcOptions.Scope.Add(OpenIdConnectScope.Profile);
            oidcOptions.Scope.Add(OpenIdConnectScope.Email);
            oidcOptions.Scope.Add(OpenIdConnectScope.Phone);
            oidcOptions.Scope.Add("roles"); // Ensure roles are included
            oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess); // added for refresh tokens.

            oidcOptions.RequireHttpsMetadata = false;
            oidcOptions.CallbackPath = builder.Configuration["Keycloak:CallbackPath"];
            oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
            oidcOptions.SaveTokens = true;
            oidcOptions.TokenValidationParameters.ValidateIssuer = true;
            oidcOptions.TokenValidationParameters.ValidateIssuerSigningKey = true;

            oidcOptions.MapInboundClaims = false;
            oidcOptions.TokenValidationParameters.NameClaimType = "name";
            oidcOptions.TokenValidationParameters.RoleClaimType = "roles";
            oidcOptions.TokenValidationParameters.ValidateAudience = true;
            oidcOptions.TokenValidationParameters.ValidateLifetime = true;
            oidcOptions.TokenValidationParameters.ValidAudience = builder.Configuration["Keycloak:Audience"];
            oidcOptions.TokenValidationParameters.ValidIssuer = builder.Configuration["Keycloak:Issuer"];
            oidcOptions.TokenValidationParameters.ClockSkew = TimeSpan.Zero;

            //events
            oidcOptions.Events = new OpenIdConnectEvents()
            {
                OnTokenValidated = context =>
                {
                    var accessToken = context.SecurityToken as JwtSecurityToken;
                    // Access the claims
                    var claims = accessToken.Claims.ToList();
                    // Do something with the claims.


                    return Task.CompletedTask;
                }
            };
            oidcOptions.Events = new OpenIdConnectEvents
            {
                OnTokenResponseReceived = context =>
                {
                    // Store the tokens in the authentication properties
                    var idToken = context.TokenEndpointResponse.IdToken;
                    var accessToken = context.TokenEndpointResponse.AccessToken;
                    var refreshToken = context.TokenEndpointResponse.RefreshToken;

                    context.Properties?.StoreTokens(new[]
                    {
                        new AuthenticationToken { Name = OpenIdConnectParameterNames.IdToken, Value = idToken },
                        new AuthenticationToken { Name = OpenIdConnectParameterNames.AccessToken, Value = accessToken },
                        new AuthenticationToken
                            { Name = OpenIdConnectParameterNames.RefreshToken, Value = refreshToken }
                    });

                    return Task.CompletedTask;
                }
            };
       
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.Name = "TexonCookie";
        //options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.ConfigureCookieOidcRefresh(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddHostedService<MyBackgroundService>();
builder.Services.AddSingleton<MyBackgroundService>();
builder.Services.AddSingleton<MyStatus>();
var app = builder.Build();
// Use the middleware
app.UseMiddleware<LogoutMiddleware>();
//app.UseMiddleware<TokenManagementMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

    

app.Run();
