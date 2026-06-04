using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MediatR;
using Wombat.Application;
using Wombat.Application.Audit;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Infrastructure;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Web.Components;
using Wombat.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSystemd();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Register OIDC providers from configuration
var ssoOptions = builder.Configuration.GetSection(SsoOptions.SectionName).Get<SsoOptions>();
if (ssoOptions?.Providers is { Count: > 0 } providers)
{
    var authBuilder = builder.Services.AddAuthentication();
    foreach (var provider in providers)
    {
        authBuilder.AddOpenIdConnect(provider.Key, provider.DisplayName, options =>
        {
            options.Authority = provider.Authority;
            options.ClientId = provider.ClientId;
            options.ClientSecret = provider.ClientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = false;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);

            options.Scope.Clear();
            foreach (var scope in provider.Scopes)
            {
                options.Scope.Add(scope);
            }

            options.CallbackPath = $"/signin-oidc-{provider.Key}";
            options.SignedOutCallbackPath = $"/signout-oidc-{provider.Key}";
        });
    }
}

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddScoped<IScopedSender, ScopedSender>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/account/login/submit", async (
    SignInManager<WombatIdentityUser> signInManager,
    UserManager<WombatIdentityUser> userManager,
    IAuditWriter auditWriter,
    HttpContext httpContext,
    [FromForm] LoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, "Email and password are required."));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;

    // Block local login for SSO-only users
    var loginUser = await userManager.FindByEmailAsync(request.Email.Trim());
    if (loginUser is not null && !loginUser.AllowLocalPassword)
    {
        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, "This account uses institutional sign-in. Please use the SSO button below."));
    }

    var result = await signInManager.PasswordSignInAsync(
        request.Email.Trim(),
        request.Password,
        request.RememberMe,
        lockoutOnFailure: false);

    if (result.Succeeded)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        var display = user is not null ? $"{user.FirstName} {user.LastName}".Trim() : null;

        await auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "Login",
            success: true,
            actorUserId: user?.Id,
            actorDisplay: display,
            actorIpAddress: ip,
            actorUserAgent: ua));

        return Results.LocalRedirect(GetSafeLocalUrl(request.ReturnUrl));
    }
    else
    {
        // Record failed login without leaking whether the user account exists.
        await auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "LoginFailed",
            success: false,
            actorIpAddress: ip,
            actorUserAgent: ua,
            errorMessage: "Invalid email or password."));

        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, "Invalid email or password."));
    }
})
.AllowAnonymous()
;

app.MapPost("/account/register/submit", async (
    ISender sender,
    UserManager<WombatIdentityUser> userManager,
    SignInManager<WombatIdentityUser> signInManager,
    [FromForm] RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Token))
    {
        return Results.LocalRedirect("/account/register?error=The%20invitation%20token%20is%20missing.");
    }

    if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
    {
        return Results.LocalRedirect(BuildRegisterUrl(request.Token, "The password confirmation does not match."));
    }

    try
    {
        var result = await sender.Send(new AcceptInvitationCommand(
            request.Token,
            request.Password ?? string.Empty,
            request.FirstName ?? string.Empty,
            request.LastName ?? string.Empty));

        var user = await userManager.FindByIdAsync(result.UserId);
        if (user is null)
        {
            return Results.LocalRedirect(BuildRegisterUrl(request.Token, "The invited user could not be loaded after registration."));
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.LocalRedirect(GetLandingPath(result.AssignedRole));
    }
    catch (Exception exception)
    {
        return Results.LocalRedirect(BuildRegisterUrl(request.Token, exception.Message));
    }
})
.AllowAnonymous()
;

app.MapGet("/account/sso-challenge/{providerKey}", (
    string providerKey,
    [FromQuery] string? returnUrl,
    IOptions<SsoOptions> ssoOpts) =>
{
    var provider = ssoOpts.Value.Providers.FirstOrDefault(
        p => string.Equals(p.Key, providerKey, StringComparison.Ordinal));

    if (provider is null)
    {
        return Results.LocalRedirect(BuildLoginUrl(null, "Unknown SSO provider."));
    }

    var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = $"/account/sso-callback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}"
    };
    properties.Items["LoginProvider"] = providerKey;

    return Results.Challenge(properties, [providerKey]);
})
.AllowAnonymous()
;

app.MapGet("/account/sso-callback", async (
    SignInManager<WombatIdentityUser> signInManager,
    ExternalLoginHandler externalLoginHandler,
    HttpContext httpContext,
    [FromQuery] string? returnUrl) =>
{
    var loginInfo = await signInManager.GetExternalLoginInfoAsync();
    if (loginInfo is null)
    {
        return Results.LocalRedirect(BuildLoginUrl(returnUrl, "External login information was not available."));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;

    var result = await externalLoginHandler.HandleCallbackAsync(loginInfo, ip, ua);

    if (result.Succeeded)
    {
        return Results.LocalRedirect(GetSafeLocalUrl(returnUrl));
    }

    if (result.RequiresLinking)
    {
        var linkUrl = $"/account/link-external?email={Uri.EscapeDataString(result.Email ?? "")}" +
                      $"&provider={Uri.EscapeDataString(result.ProviderKey ?? "")}" +
                      $"&externalId={Uri.EscapeDataString(result.ExternalSubjectId ?? "")}" +
                      $"&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
        return Results.LocalRedirect(linkUrl);
    }

    return Results.LocalRedirect(BuildLoginUrl(returnUrl, result.ErrorMessage ?? "SSO login failed."));
})
.AllowAnonymous()
;

app.MapPost("/account/link-external/submit", async (
    SignInManager<WombatIdentityUser> signInManager,
    ExternalLoginHandler externalLoginHandler,
    HttpContext httpContext,
    [FromForm] LinkExternalRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.LocalRedirect(
            $"/account/link-external?email={Uri.EscapeDataString(request.Email ?? "")}" +
            $"&provider={Uri.EscapeDataString(request.ProviderKey ?? "")}" +
            $"&externalId={Uri.EscapeDataString(request.ExternalSubjectId ?? "")}" +
            $"&error={Uri.EscapeDataString("Email and password are required.")}");
    }

    var loginInfo = await signInManager.GetExternalLoginInfoAsync();
    if (loginInfo is null)
    {
        return Results.LocalRedirect(BuildLoginUrl(null, "External login session expired. Please try again."));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;

    var result = await externalLoginHandler.LinkAndSignInAsync(
        request.Email, request.Password, request.ProviderKey ?? "", request.ExternalSubjectId ?? "",
        loginInfo, ip, ua);

    if (result.Succeeded)
    {
        return Results.LocalRedirect(GetSafeLocalUrl(request.ReturnUrl));
    }

    return Results.LocalRedirect(
        $"/account/link-external?email={Uri.EscapeDataString(request.Email ?? "")}" +
        $"&provider={Uri.EscapeDataString(request.ProviderKey ?? "")}" +
        $"&externalId={Uri.EscapeDataString(request.ExternalSubjectId ?? "")}" +
        $"&error={Uri.EscapeDataString(result.ErrorMessage ?? "Linking failed.")}");
})
.AllowAnonymous()
;

app.MapPost("/account/logout", async (
    SignInManager<WombatIdentityUser> signInManager,
    IAuditWriter auditWriter,
    HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var display = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

    await signInManager.SignOutAsync();

    await auditWriter.WriteAsync(AuditEntry.Create(
        occurredAt: DateTime.UtcNow,
        category: AuditCategory.Authentication,
        action: "Logout",
        success: true,
        actorUserId: userId,
        actorDisplay: display,
        actorIpAddress: TruncateLoginIp(httpContext.Connection.RemoteIpAddress)));

    return Results.LocalRedirect("/account/login");
});

app.MapGet("/account/data-rights/download/{id:guid}", async (
    Guid id,
    ISender sender,
    HttpContext httpContext) =>
{
    try
    {
        var result = await sender.Send(
            new Wombat.Application.Features.DataRights.Queries.DownloadAccessReportQuery(id, httpContext.User));
        return Results.File(result.ZipBytes, "application/zip", result.FileName);
    }
    catch (Exception exception) when (
        exception is UnauthorizedAccessException ||
        exception is InvalidOperationException)
    {
        // 404 (not 403) so we do not leak the existence of other users' requests, mirroring the
        // institution-scope convention. Covers: not found, wrong type, not yet completed, not owner.
        return Results.NotFound();
    }
})
.RequireAuthorization();

app.MapGet("/dashboard/switch/{role}", (string role, HttpContext httpContext) =>
{
    if (Wombat.Web.Navigation.DashboardPriority.ValidRoles.Contains(role))
    {
        httpContext.Response.Cookies.Append(
            Wombat.Web.Navigation.DashboardPriority.CookieName,
            role,
            new CookieOptions
            {
                SameSite = SameSiteMode.Lax,
                HttpOnly = true,
                Secure = !httpContext.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase),
                MaxAge = TimeSpan.FromDays(30)
            });
    }
    return Results.LocalRedirect("/");
});

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();

    await dbContext.Database.MigrateAsync();

    if (args.Contains("--migrate", StringComparer.Ordinal))
    {
        return;
    }

    await roleSeeder.SeedAsync();
    await adminSeeder.SeedAsync();
    await dataSeeder.SeedAsync();

    if (app.Environment.IsDevelopment())
    {
        var devUserSeeder = scope.ServiceProvider.GetRequiredService<DevUserSeeder>();
        await devUserSeeder.SeedAsync();
    }
}

if (args.Contains("--seed", StringComparer.Ordinal))
{
    return;
}

app.Run();

static string GetSafeLocalUrl(string? url)
{
    if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Relative, out _))
    {
        return "/";
    }

    return url;
}

static string BuildLoginUrl(string? returnUrl, string error)
{
    var query = $"error={Uri.EscapeDataString(error)}";

    if (!string.IsNullOrWhiteSpace(returnUrl))
    {
        query += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
    }

    return $"/account/login?{query}";
}

static string BuildRegisterUrl(string token, string error)
    => $"/account/register?token={Uri.EscapeDataString(token)}&error={Uri.EscapeDataString(error)}";

static string? TruncateLoginIp(System.Net.IPAddress? address)
{
    if (address is null) return null;
    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    {
        var bytes = address.GetAddressBytes();
        bytes[3] = 0;
        return new System.Net.IPAddress(bytes).ToString() + "/24";
    }
    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
    {
        var bytes = address.GetAddressBytes();
        for (int i = 6; i < 16; i++) bytes[i] = 0;
        return new System.Net.IPAddress(bytes).ToString() + "/48";
    }
    return address.ToString();
}

static string GetLandingPath(string role)
    => string.Equals(role, WombatRoles.Administrator, StringComparison.Ordinal)
        ? "/admin/invitations"
        : string.Equals(role, WombatRoles.PendingTrainee, StringComparison.Ordinal)
            ? "/"
        : "/";

internal sealed class LoginRequest
{
    public string? Email { get; init; }
    public string? Password { get; init; }
    public bool RememberMe { get; init; }
    public string? ReturnUrl { get; init; }
}

internal sealed class RegisterRequest
{
    public string? Token { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Password { get; init; }
    public string? ConfirmPassword { get; init; }
}

internal sealed class LinkExternalRequest
{
    public string? Email { get; init; }
    public string? Password { get; init; }
    public string? ProviderKey { get; init; }
    public string? ExternalSubjectId { get; init; }
    public string? ReturnUrl { get; init; }
}
