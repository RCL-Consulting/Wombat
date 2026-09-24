using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MediatR;
using Wombat.Application;
using Wombat.Application.Audit;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Infrastructure;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Web.Components;
using Wombat.Web.Security;
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
builder.Services.AddWombatCircuitServices();
builder.Services.AddHttpContextAccessor();

// Health: a liveness-only probe reports "Healthy" while PostgreSQL is unreachable, which
// would let the health cron skip its restart and let a deploy gate report success against
// an unusable app. Probe the database the app actually depends on.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

const string LoginRateLimitPolicy = "login";

// Login throttling. Identity lockout (see AddInfrastructure) caps attempts per *account*;
// this caps them per client IP so an attacker cannot spread a password-spray across many
// accounts, and cannot lock a legitimate user out by burning their attempts for them.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(LoginRateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: TruncateLoginIp(httpContext.Connection.RemoteIpAddress) ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
            }));

    // The MSF respondent page (/msf/respond): ten requests a minute for one link from one address, and
    // sixty from one address whatever the links (a global limiter confined to the page's route). The
    // Api's respond endpoint carries the same, from the one definition both hosts register (T205).
    options.AddMsfRespondPolicy();

    // This endpoint is a browser form post, so a bare 429 would render as a blank error
    // page. Redirect (302) back to the login form with an explanation instead, and set
    // Retry-After so non-browser clients still learn how long to wait.
    //
    // Except a respondent's link: they are not signed in and have no account, so the sign-in
    // page is the wrong answer. They get a 429 that says what happened (T205).
    options.OnRejected = (context, cancellationToken) =>
    {
        if (MsfRespondRateLimit.Limits(context.HttpContext))
        {
            return MsfRespondThrottle.RefuseAsync(context.HttpContext, cancellationToken);
        }

        context.HttpContext.Response.Headers.RetryAfter = "300";
        context.HttpContext.Response.Redirect(
            BuildLoginUrl(null, "Too many sign-in attempts. Please wait a few minutes and try again."));
        return ValueTask.CompletedTask;
    };
});

// DataProtection: persist keys so auth cookies / antiforgery tokens survive restarts.
// Under systemd (ProtectSystem=strict + a homeless service user) the default key
// location ($HOME/.aspnet) is not writable, so keys would be ephemeral and every
// restart would log users out. When a writable keys path is configured, persist there.
var dataProtectionBuilder = builder.Services.AddDataProtection().SetApplicationName("Wombat");
var dataProtectionKeysPath = builder.Configuration["Wombat:DataProtectionKeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    System.IO.Directory.CreateDirectory(dataProtectionKeysPath);
    dataProtectionBuilder.PersistKeysToFileSystem(new System.IO.DirectoryInfo(dataProtectionKeysPath));
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Running behind Caddy: TLS is terminated at the proxy and traffic is forwarded over
// loopback as plain HTTP. Honor X-Forwarded-Proto/-For (set by the Caddyfile) so the
// app sees the real https scheme and client IP. Must run before authentication so the
// Identity/antiforgery cookies are issued Secure and any redirects use https.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Security headers (CSP nonce, nosniff, referrer policy) — before anything that can write
// a response, so static files and error pages carry them too.
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapHealthChecks("/health").AllowAnonymous();

// MapStaticAssets().AllowAnonymous(). The opt-out is required: the fallback policy requires a
// signed-in user on every endpoint, the fingerprinted assets included, so without it the sign-in
// page's own stylesheets and scripts redirect to /account/login (T175). Call this, never a bare
// MapStaticAssets(): Hosting/AppAssetUrlTests maps the same method under the same policy.
app.MapWombatStaticAssets();
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
        lockoutOnFailure: true);

    if (result.Succeeded)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        var display = user is not null ? $"{user.FirstName} {user.LastName}".Trim() : null;

        // Stamped so an InstitutionalAdmin keeps sight of their own users' sign-ins. T101 removed
        // the "null institution means everyone may read it" catch-all from the audit queries, so an
        // unstamped row is now Administrator-only. The two FAILED paths below stay unstamped on
        // purpose: resolving an institution there would confirm the account exists, which is exactly
        // what their generic messages are written to avoid.
        await auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "Login",
            success: true,
            actorUserId: user?.Id,
            actorDisplay: display,
            actorIpAddress: ip,
            actorUserAgent: ua,
            institutionId: user?.InstitutionId));

        return Results.LocalRedirect(GetSafeLocalUrl(request.ReturnUrl));
    }
    else if (result.IsLockedOut)
    {
        // Distinguished in the audit log so an admin can see a lockout trip, but the
        // user-facing message stays generic: saying "this account is locked" would
        // confirm the address exists.
        //
        // Stamped, unlike LoginFailed below. A lockout only trips on an account that exists, and it
        // can only ever be that account's own institution — so naming the locked-out user to their
        // own InstitutionalAdmin discloses nothing they cannot already read off /admin/users, while
        // leaving the row unstamped made it Administrator-only and defeated the reason it is written.
        // The generic response above is unchanged; this is the log, not the reply. (T101)
        await auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "LoginLockedOut",
            success: false,
            actorUserId: loginUser?.Id,
            actorDisplay: loginUser is not null ? $"{loginUser.FirstName} {loginUser.LastName}".Trim() : null,
            actorIpAddress: ip,
            actorUserAgent: ua,
            institutionId: loginUser?.InstitutionId,
            errorMessage: "Account locked after repeated failed sign-in attempts."));

        return Results.LocalRedirect(BuildLoginUrl(
            request.ReturnUrl,
            "Too many failed sign-in attempts. Please try again later or reset your password."));
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
.RequireRateLimiting(LoginRateLimitPolicy)
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

    // The external cookie is only needed while a link is pending. On every final outcome it goes, so a refused or
    // finished sign-in cannot be replayed from it (T149).
    if (!result.RequiresLinking)
    {
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    if (result.Succeeded)
    {
        return Results.LocalRedirect(GetSafeLocalUrl(returnUrl));
    }

    if (result.RequiresLinking)
    {
        // Only what the page shows and where to go next. What gets linked is read back from the external cookie at
        // submit, never from the page (T149).
        return Results.LocalRedirect(BuildLinkUrl(returnUrl, error: null));
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
    // The external login is the whole identity of the request: the provider, the subject and the account's email are
    // read from it, never from the form (T149).
    var loginInfo = await signInManager.GetExternalLoginInfoAsync();
    if (loginInfo is null)
    {
        return Results.LocalRedirect(BuildLoginUrl(null, "External login session expired. Please try again."));
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.LocalRedirect(BuildLinkUrl(request.ReturnUrl, "Your password is required."));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;

    var result = await externalLoginHandler.LinkAndSignInAsync(loginInfo, request.Password, ip, ua);

    if (result.Succeeded)
    {
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Results.LocalRedirect(GetSafeLocalUrl(request.ReturnUrl));
    }

    return Results.LocalRedirect(BuildLinkUrl(request.ReturnUrl, result.ErrorMessage ?? "Linking failed."));
})
.AllowAnonymous()
// A password check, so the local login's throttle applies: per-IP, 10 per five minutes. The per-account lockout is the
// handler's (lockoutOnFailure). Before T149 this endpoint had neither. (T149)
.RequireRateLimiting(LoginRateLimitPolicy)
;

app.MapPost("/account/logout", async (
    SignInManager<WombatIdentityUser> signInManager,
    IAuditWriter auditWriter,
    HttpContext httpContext) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var display = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    // Read BEFORE SignOutAsync — afterwards the principal is gone and the row would be unstamped,
    // i.e. Administrator-only under T101's audit scoping.
    var institutionId = httpContext.User.GetInstitutionId();

    await signInManager.SignOutAsync();

    await auditWriter.WriteAsync(AuditEntry.Create(
        occurredAt: DateTime.UtcNow,
        category: AuditCategory.Authentication,
        action: "Logout",
        success: true,
        actorUserId: userId,
        actorDisplay: display,
        actorIpAddress: TruncateLoginIp(httpContext.Connection.RemoteIpAddress),
        institutionId: institutionId));

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
    await scope.ServiceProvider.GetRequiredService<PaediatricCatalogueSeeder>().SeedAsync();

    // Both seeders above skip keys that already exist, so a seed-file edit only ever reached a
    // fresh database. This carries those edits into existing types by publishing a new version
    // (T103). It never throws and never reverts operator work; see ActivityTypeSeedRefresher.
    await scope.ServiceProvider.GetRequiredService<ActivityTypeSeedRefresher>().RefreshAsync();


    if (app.Environment.IsDevelopment())
    {
        var devUserSeeder = scope.ServiceProvider.GetRequiredService<DevUserSeeder>();
        await devUserSeeder.SeedAsync();
    }

    // The T130 migration empties the progress table, because a lifetime tally cannot be split into
    // semesters. This refills it from the completed activities, once, whenever the table is empty but
    // completions record credit. It never throws; see CurriculumProgressBootstrapper.
    await scope.ServiceProvider.GetRequiredService<CurriculumProgressBootstrapper>().RunAsync();
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

// No email in the URL: the page reads it from the sign-in in progress, so a crafted link cannot show one address while
// another is linked, and the address stays out of the proxy's access log (T149).
static string BuildLinkUrl(string? returnUrl, string? error)
    => $"/account/link-external?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}" +
       (string.IsNullOrWhiteSpace(error) ? string.Empty : $"&error={Uri.EscapeDataString(error)}");

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

/// <summary>
/// The link form. Only the password and where to go next: which login is linked to which account is read from the
/// external cookie (T149).
/// </summary>
internal sealed class LinkExternalRequest
{
    public string? Password { get; init; }
    public string? ReturnUrl { get; init; }
}

/// <summary>
/// Public so the integration suite can host this app whole (<c>WebApplicationFactory</c>), under an alias: the Api's
/// <c>Program</c> is public too. The MSF respondent page is tested through it, from the link to the stored response (T205).
/// </summary>
public partial class Program;
