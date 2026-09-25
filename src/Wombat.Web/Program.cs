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
using Microsoft.Extensions.Options;
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
using Wombat.Web.Navigation;
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

            // The email's verification decides whether SSO may write the email or provision an account (T155). The ID
            // token's claims reach the external login, but from the userinfo response the handler keeps only the
            // claims ClaimActions names, and the defaults do not name this one. A provider that asserts it only there
            // would otherwise never be seen to verify anything.
            if (!string.IsNullOrWhiteSpace(provider.EmailVerifiedClaim))
            {
                options.ClaimActions.MapUniqueJsonKey(provider.EmailVerifiedClaim, provider.EmailVerifiedClaim);
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

// The sign-in throttle: ten failed password checks in five minutes from one client address, shared by the sign-in, link
// and change-password endpoints below. Identity's lockout (see AddInfrastructure) caps the guesses at one account; this
// caps them from one client, so a password spray across many accounts is slowed too. Only failures count, so the people
// behind one hospital's address do not share ten sign-ins between them (T156). Each endpoint counts its own password
// check: a rate-limiter policy counts a request before the endpoint runs, and cannot tell a failure from a success.
builder.Services.AddSingleton<SignInThrottle>();

builder.Services.AddRateLimiter(options =>
{
    // The MSF respondent page (/msf/respond): ten requests a minute for one link from one address, and
    // sixty from one address whatever the links (a global limiter confined to the page's route). The
    // Api's respond endpoint carries the same, from the one definition both hosts register (T205).
    options.AddMsfRespondPolicy();

    // Its limits are the host's only ones. A respondent is not signed in and has no account, so they get a 429 that says
    // what happened, not a redirect to a sign-in page (T205).
    options.OnRejected = (context, cancellationToken) => MsfRespondThrottle.RefuseAsync(context.HttpContext, cancellationToken);
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
//
// Only from Caddy, which reaches Kestrel over loopback (ASPNETCORE_URLS=http://127.0.0.1:5080). Until T156 the headers
// were believed from any peer, which was safe only while Kestrel listened on loopback alone: bound anywhere else, any
// client could name the address the sign-in throttle counts, and the scheme the cookies are issued for. From any other
// peer they are ignored, and the peer's own address and scheme stand. The middleware reads an IPv4 address mapped into
// IPv6 as the IPv4 one, so a dual-stack socket's ::ffff:127.0.0.1 is loopback too.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
forwardedHeadersOptions.KnownProxies.Add(System.Net.IPAddress.Loopback);
forwardedHeadersOptions.KnownProxies.Add(System.Net.IPAddress.IPv6Loopback);
app.UseForwardedHeaders(forwardedHeadersOptions);

// Security headers (CSP nonce, nosniff, referrer policy) — before anything that can write
// a response, so static files and error pages carry them too.
app.UseMiddleware<SecurityHeadersMiddleware>();

// A browser's 404 is rerun as /not-found, the app's own page, still answered 404. Only a GET that accepts HTML for an
// address that is not a file: every other status, and every other request, is answered as before (T233). After the
// security headers, so the page carries them and its nonce; before static files and routing, so the rerun is routed.
app.UseWombatNotFoundPage();

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

// MapRazorComponents<App>().AddInteractiveServerRenderMode(), with /_blazor/initializers, a list of public module names,
// open to a visitor who has not signed in. The circuit stays behind the fallback policy, and App.razor gives such a
// visitor no interactive root, so their browser never asks for one (T181). Call this, never a bare MapRazorComponents:
// Hosting/BlazorEndpointAccessTests maps the same method under the same policy.
app.MapWombatRazorComponents();

app.MapPost("/account/login/submit", async (
    SignInManager<WombatIdentityUser> signInManager,
    UserManager<WombatIdentityUser> userManager,
    IAuditWriter auditWriter,
    SignInThrottle throttle,
    IOptions<SsoOptions> ssoOpts,
    HttpContext httpContext,
    [FromForm] LoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, "Email and password are required."));
    }

    // Counted before the account is looked up, so every refusal below costs the client one of its ten (T156).
    var attempt = throttle.Begin(httpContext);
    if (attempt.Refused)
    {
        SignInThrottle.SetRetryAfter(httpContext.Response, attempt);
        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, SignInMessages.TooManyFailedAttempts));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;
    var refused = SignInMessages.Refused(institutionalSignInOffered: ssoOpts.Value.Providers.Count > 0);

    // An account that signs in only through its institution has no password to check, and a check could only count
    // failures towards a lockout that would block its SSO sign-in too. It is refused as an address no account has is, in
    // the same words and with the same audit row: telling it apart would say which addresses have accounts, and how they
    // sign in (T156).
    var loginUser = await userManager.FindByEmailAsync(request.Email.Trim());
    if (loginUser is not null && !loginUser.AllowLocalPassword)
    {
        await WriteLoginFailedAsync(auditWriter, ip, ua);
        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, refused));
    }

    var result = await signInManager.PasswordSignInAsync(
        request.Email.Trim(),
        request.Password,
        request.RememberMe,
        lockoutOnFailure: true);

    if (result.Succeeded)
    {
        attempt.Release();

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
        // Distinguished in the audit log so an admin can see a lockout trip. The reply names no account, but it is not
        // the reply an unknown address gets, so it does say one exists: Identity answers a locked account before it
        // checks a password, so an account an administrator has deactivated gets it at the first try, and an active one
        // after five wrong guesses. An unknown or institutional address never does. Left so, knowingly (T156 review):
        // whether a locked-out person is told so is its own decision.
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
        await WriteLoginFailedAsync(auditWriter, ip, ua);

        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, refused));
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
    SignInThrottle throttle,
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

    // A password check, so the sign-in throttle applies, shared with the sign-in page: ten failures in five minutes from
    // one address. The per-account lockout is the handler's (lockoutOnFailure). Before T149 this endpoint had neither.
    //
    // A refusal ends the institutional sign-in in progress and goes to the sign-in page, which says to wait and try
    // again. The external cookie does not slide, and ends five minutes after the callback (T156), while the client's
    // window can run five more, so a person told to wait here would come back to a sign-in that had expired. They start
    // again from their institution's button once the window has passed (T156 review).
    var attempt = throttle.Begin(httpContext);
    if (attempt.Refused)
    {
        SignInThrottle.SetRetryAfter(httpContext.Response, attempt);
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Results.LocalRedirect(BuildLoginUrl(request.ReturnUrl, SignInMessages.TooManyFailedAttempts));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;

    var result = await externalLoginHandler.LinkAndSignInAsync(loginInfo, request.Password, ip, ua);

    if (result.Succeeded)
    {
        attempt.Release();
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Results.LocalRedirect(GetSafeLocalUrl(request.ReturnUrl));
    }

    return Results.LocalRedirect(BuildLinkUrl(request.ReturnUrl, result.ErrorMessage ?? "Linking failed."));
})
.AllowAnonymous()
;

// The change-password page's form posts here (T265). Changing a password changes the account's security stamp, which the
// sign-in cookie carries, so the cookie is issued again here, in the same request, while its response can still set it.
// Until T265 the page did both in its circuit, whose response had started long before: the password changed, and writing
// the cookie threw "Headers are read-only".
//
// The change and the new cookie are one request on purpose. An endpoint that only issued the cookie again, called after
// the circuit had changed the password, would do it for any cookie still inside the stamp validator's interval, a stolen
// one included, and hand it the new stamp: the change would not end the session it was made to end.
//
// For the same reason the session itself is checked first. The stamp validator looks at a cookie once in thirty minutes,
// so between its checks a session that has already ended (an administrator's lock, a change made in another browser, a
// change of roles) still reaches this endpoint. Such a session is signed out, not issued a cookie carrying the new stamp,
// which would have kept it alive past the lock for as long as it was used (T265 review).
//
// The current password is checked as the sign-in page checks it: with Identity's lockout, and under the sign-in throttle.
// Someone holding a stolen cookie could otherwise guess at it without limit, and one hit would give them the account.
//
// Every outcome is a redirect: back to the page, which says what happened (ChangePasswordOutcome), or to the sign-in page.
app.MapPost(ChangePasswordOutcome.SubmitPath, async (
    SignInManager<WombatIdentityUser> signInManager,
    UserManager<WombatIdentityUser> userManager,
    ILoggerFactory loggerFactory,
    SignInThrottle throttle,
    HttpContext httpContext,
    [FromForm] ChangePasswordRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.CurrentPassword)
        || string.IsNullOrWhiteSpace(request.NewPassword)
        || string.IsNullOrWhiteSpace(request.ConfirmPassword))
    {
        return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.FieldsMissing]));
    }

    if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
    {
        return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.ConfirmationMismatch]));
    }

    // Everything up to and including the change. A fault here (the database gone) has changed nothing, so the page says
    // the password could not be changed, rather than the request ending on the error page.
    WombatIdentityUser user;
    try
    {
        // The account, only if the cookie's stamp is still the account's. Null for a stamp that has changed since the
        // cookie was issued, or an account that is gone.
        var current = await signInManager.ValidateSecurityStampAsync(httpContext.User);
        if (current is null)
        {
            await signInManager.SignOutAsync();
            return Results.LocalRedirect(
                BuildLoginUrl(ChangePasswordOutcome.PagePath, ChangePasswordOutcome.SessionEndedMessage));
        }

        user = current;

        // Its password does not sign it in, and an SSO-provisioned account has none: a check could only count failures
        // towards a lockout, which would block its SSO sign-in too.
        if (!user.AllowLocalPassword)
        {
            return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.InstitutionalSignIn]));
        }

        // The sign-in throttle, as the sign-in and link pages apply it: ten failed password checks in five minutes from one
        // address, shared with them. A refusal comes back to this page, which says why (T265, T156).
        var attempt = throttle.Begin(httpContext);
        if (attempt.Refused)
        {
            SignInThrottle.SetRetryAfter(httpContext.Response, attempt);
            return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.TooManyAttempts]));
        }

        // A locked account's password is not checked at all, and a fifth wrong one locks it.
        var check = await signInManager.CheckPasswordSignInAsync(user, request.CurrentPassword, lockoutOnFailure: true);
        if (check.Succeeded)
        {
            attempt.Release();
        }

        if (check.IsLockedOut)
        {
            return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.LockedOut]));
        }

        if (!check.Succeeded)
        {
            return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl(check.IsNotAllowed
                ? [ChangePasswordOutcome.Failed]
                : [nameof(IdentityErrorDescriber.PasswordMismatch)]));
        }

        // Changes the password and the security stamp together, or changes nothing. A change made elsewhere since the
        // account was read fails on the row's concurrency stamp (ConcurrencyFailure, the general refusal); it is not
        // overwritten.
        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl(result.Errors.Select(error => error.Code)));
        }
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        loggerFactory.CreateLogger(ChangePasswordOutcome.LogCategory)
            .LogError(exception, "A password change failed before the password was changed.");
        return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.Failed]));
    }

    // The cookie again, with the new stamp, keeping the old one's "remember me" and how the user signed in. The password
    // has changed by now, so a fault here must not say it could not be: the old cookie's stamp is stale, so it is taken
    // away, and the sign-in page asks for the new password.
    try
    {
        await signInManager.RefreshSignInAsync(user);
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        loggerFactory.CreateLogger(ChangePasswordOutcome.LogCategory)
            .LogError(exception, "A password was changed, but the sign-in cookie could not be issued again.");
        await signInManager.SignOutAsync();
        return Results.LocalRedirect(BuildLoginUrl(null, ChangePasswordOutcome.ChangedSignInAgainMessage));
    }

    return Results.LocalRedirect(ChangePasswordOutcome.UpdatedUrl);
})
.RequireAuthorization();

// Where the sign-in page sends a user whose session had ended before they pressed Change password: the post was
// challenged, and the address it was going to comes back as the sign-in's return address, as a GET. Without this it
// answered 405 with an empty page (T265 review).
app.MapGet(ChangePasswordOutcome.SubmitPath, () => Results.LocalRedirect(ChangePasswordOutcome.PagePath))
    .RequireAuthorization();

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

// The row every refused sign-in writes, but a lockout's: unstamped and naming no account, whatever the cause, so the log
// says no more than the page about which addresses have accounts (T101, T156).
static Task WriteLoginFailedAsync(IAuditWriter auditWriter, string? ip, string? ua)
    => auditWriter.WriteAsync(AuditEntry.Create(
        occurredAt: DateTime.UtcNow,
        category: AuditCategory.Authentication,
        action: "LoginFailed",
        success: false,
        actorIpAddress: ip,
        actorUserAgent: ua,
        errorMessage: SignInMessages.InvalidCredentials));

// The address an audit row keeps: its /24 (IPv6: its /48), never the whole of it. The sign-in throttle counts the whole
// address (ClientAddress), in memory only.
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

/// <summary>The change-password form (T265). The account is the signed-in user's, never the form's.</summary>
internal sealed class ChangePasswordRequest
{
    public string? CurrentPassword { get; init; }
    public string? NewPassword { get; init; }
    public string? ConfirmPassword { get; init; }
}

/// <summary>
/// Public so the integration suite can host this app whole (<c>WebApplicationFactory</c>), under an alias: the Api's
/// <c>Program</c> is public too. The MSF respondent page is tested through it, from the link to the stored response (T205).
/// </summary>
public partial class Program;
