using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
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
using Wombat.Application.Features.Accounts;
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

// A circuit's sign-in is checked against the account every minute, as the cookie's is between requests, so a lock, an
// erasure or a change of roles reaches a tab already open (T279). See SessionRevalidation.
builder.Services.AddScoped<AuthenticationStateProvider, SessionRevalidatingAuthenticationStateProvider>();
builder.Services.AddWombatCircuitServices();
builder.Services.AddHttpContextAccessor();

// The acting role's switch and the one-time word it leaves for the page it lands on (T335, flow 01).
builder.Services.AddActingRoleSwitch();

// A failed page load's reference goes to the log with the failure, as the error page shows it (T321; ErrorPages).
builder.Services.AddExceptionHandler<ErrorPages.ReferenceLog>();

// The error page's rerun checks the sign-in cookie again; with the database down that check throws, and the rerun draws
// the page signed out rather than fail with it (ErrorPages.GuardTheSignInCheck). After AddInfrastructure, whose Identity
// sets the check this wraps.
builder.Services.ConfigureApplicationCookie(ErrorPages.GuardTheSignInCheck);

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

// SignInTiming, the password check that checks nothing so a refused sign-in takes as long whatever refused it (T339,
// flow 02, E5), is registered by AddInfrastructure: the link page's handler uses it too.

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

// A browser's failed page load is rerun as /Error, the app's own page, still answered 500, with the reference the log
// holds; every other failed request keeps its bare 500 (T321; ErrorPages). Outside Development, whose developer exception
// page shows the failure itself. Before the security headers and the 404 rerun: the handler clears the failed response,
// headers and all, so its rerun passes through both again, and a failure inside the 404's rerun is caught too.
if (!app.Environment.IsDevelopment())
{
    app.UseWombatErrorPage();
}

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

// Every refusal below goes back to the sign-in page with a code, never a sentence: the page chooses the words
// (SignInOutcome), so a crafted link cannot put words of its own on it (T285). So do the institutional sign-in's, the link
// page's lockout and change password's.
app.MapPost("/account/login/submit", async (
    SignInManager<WombatIdentityUser> signInManager,
    UserManager<WombatIdentityUser> userManager,
    IAuditWriter auditWriter,
    SignInThrottle throttle,
    SignInTiming timing,
    HttpContext httpContext,
    [FromForm] LoginRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.FieldsMissing, request.ReturnUrl));
    }

    // Counted before the account is looked up, so every refusal below costs the client one of its ten (T156).
    var attempt = throttle.Begin(httpContext);
    if (attempt.Refused)
    {
        SignInThrottle.SetRetryAfter(httpContext.Response, attempt);
        return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.TooManyAttempts, request.ReturnUrl));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;

    // The words are the page's, which point at the institutional sign-in button where the page offers one
    // (SignInMessages.Refused). A wrong password, an address no account has, an account that signs in only through its
    // institution and a locked account are all sent here, to the same address, and each after one password check's time
    // (T287 and its timing half, T339, flow 02, E5): neither the address nor a stopwatch says which it was.
    var refused = SignInOutcome.Url(SignInOutcome.Refused, request.ReturnUrl);

    // An address no account has: Identity would find no user and answer at once, before any password check.
    var loginUser = await userManager.FindByEmailAsync(request.Email.Trim());
    if (loginUser is null)
    {
        timing.Equalize(userManager.PasswordHasher, request.Password);
        await WriteLoginFailedAsync(auditWriter, ip, ua);
        return Results.LocalRedirect(refused);
    }

    // An account that signs in only through its institution has no password to check, and a check could only count
    // failures towards a lockout that would block its SSO sign-in too. It is refused as an address no account has is, in
    // the same words and with the same audit row: telling it apart would say which addresses have accounts, and how they
    // sign in (T156).
    if (!loginUser.AllowLocalPassword)
    {
        timing.Equalize(userManager.PasswordHasher, request.Password);
        await WriteLoginFailedAsync(auditWriter, ip, ua);
        return Results.LocalRedirect(refused);
    }

    // Identity answers an account that is locked already before it checks the password; the attempt that trips the lock
    // has checked one. So only the first costs nothing, and is made to.
    var lockedBefore = await userManager.IsLockedOutAsync(loginUser);

    var result = await signInManager.PasswordSignInAsync(
        loginUser,
        request.Password,
        request.RememberMe,
        lockoutOnFailure: true);

    if (result.Succeeded)
    {
        attempt.Release();

        var user = loginUser;
        var display = $"{user.FirstName} {user.LastName}".Trim();

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
            actorUserId: user.Id,
            actorDisplay: display,
            actorIpAddress: ip,
            actorUserAgent: ua,
            institutionId: user.InstitutionId));

        // Followed only when it is a path on this site. Until T335 the test was Uri.TryCreate(…, Relative), which passes
        // //evil.example and /\evil.example: LocalRedirect then threw, and the sign-in answered 500 (LocalUrl). The
        // institutional sign-in and the link page follow theirs the same way.
        return Results.LocalRedirect(LocalUrl.OrNull(request.ReturnUrl) ?? "/");
    }
    else if (result.IsLockedOut)
    {
        if (lockedBefore)
        {
            timing.Equalize(userManager.PasswordHasher, request.Password);
        }

        // Distinguished in the audit log so an admin can see a lockout trip. The reply is a wrong password's, words and
        // address alike (T287, T339, flow 02, the round 2 review's A1 and A2). Until T339 it was a code of its own, which
        // said an account existed: Identity answers a locked account before it checks a password, so an account an
        // administrator has deactivated got it at the first try, and an active one after five wrong guesses, while an
        // unknown or institutional address never did.
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
            actorUserId: loginUser.Id,
            actorDisplay: $"{loginUser.FirstName} {loginUser.LastName}".Trim(),
            actorIpAddress: ip,
            actorUserAgent: ua,
            institutionId: loginUser.InstitutionId,
            errorMessage: "Account locked after repeated failed sign-in attempts."));

        return Results.LocalRedirect(refused);
    }
    else
    {
        // A wrong password has also written the account's failed-attempt count (AccessFailedAsync), one database write
        // the refusals above do not make: a residual timing difference, far smaller than the hash, and accepted (the
        // review of the t339 branch; SignInTiming's remarks).
        //
        // Record failed login without leaking whether the user account exists.
        await WriteLoginFailedAsync(auditWriter, ip, ua);

        return Results.LocalRedirect(refused);
    }
})
.AllowAnonymous()
;

// A refusal goes back to the register page as codes (RegisterOutcome), never as a sentence, and never as an exception's
// message: a fault is logged, and the page says registration could not be completed (T285).
app.MapPost(RegisterOutcome.SubmitPath, async (
    ISender sender,
    UserManager<WombatIdentityUser> userManager,
    SignInManager<WombatIdentityUser> signInManager,
    ILoggerFactory loggerFactory,
    [FromForm] RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Token))
    {
        return Results.LocalRedirect(RegisterOutcome.Url(null, [RegisterOutcome.TokenMissing]));
    }

    if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
    {
        return Results.LocalRedirect(RegisterOutcome.Url(request.Token, [RegisterOutcome.ConfirmationMismatch]));
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
            return Results.LocalRedirect(RegisterOutcome.Url(request.Token, [RegisterOutcome.UserNotLoaded]));
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.LocalRedirect(GetLandingPath(result.AssignedRole));
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        var codes = RegisterOutcome.CodesFor(exception);
        var logger = loggerFactory.CreateLogger(RegisterOutcome.LogCategory);
        if (RegisterOutcome.IsRefusal(exception))
        {
            logger.LogInformation("A registration was refused ({Codes}).", string.Join(", ", codes));
        }
        else
        {
            logger.LogError(exception, "A registration failed.");
        }

        return Results.LocalRedirect(RegisterOutcome.Url(request.Token, codes));
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
        return Results.LocalRedirect(SignInOutcome.Url(ExternalLoginRefusal.UnknownProvider));
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
        return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.ExternalLoginUnavailable, returnUrl));
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
        return Results.LocalRedirect(LocalUrl.OrNull(returnUrl) ?? "/");
    }

    if (result.RequiresLinking)
    {
        // Only what the page shows and where to go next. What gets linked is read back from the external cookie at
        // submit, never from the page (T149).
        return Results.LocalRedirect(LinkExternalOutcome.Url(returnUrl, code: null));
    }

    return Results.LocalRedirect(SignInOutcome.Url(result.ErrorCode ?? SignInOutcome.SsoFailed, returnUrl));
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
        return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.ExternalSessionExpired));
    }

    if (string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.LocalRedirect(LinkExternalOutcome.Url(request.ReturnUrl, LinkExternalOutcome.PasswordRequired));
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
        return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.TooManyAttempts, request.ReturnUrl));
    }

    var ip = TruncateLoginIp(httpContext.Connection.RemoteIpAddress);
    var ua = httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } s ? s : null;

    var result = await externalLoginHandler.LinkAndSignInAsync(loginInfo, request.Password, ip, ua);

    if (result.Succeeded)
    {
        attempt.Release();
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Results.LocalRedirect(LocalUrl.OrNull(request.ReturnUrl) ?? "/");
    }

    return Results.LocalRedirect(
        LinkExternalOutcome.Url(request.ReturnUrl, result.ErrorCode ?? ExternalLoginRefusal.LinkFailed));
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
// For the same reason the session itself is checked first. The stamp validator looks at a cookie once a minute
// (SessionRevalidation, T279), so between its checks a session that has already ended (an administrator's lock, a change
// made in another browser, a change of roles) still reaches this endpoint. Such a session is signed out, not issued a cookie carrying the new stamp,
// which would have kept it alive past the lock for as long as it was used (T265 review).
//
// The current password is checked as the sign-in page checks it: with Identity's lockout, and under the sign-in throttle.
// Someone holding a stolen cookie could otherwise guess at it without limit, and one hit would give them the account.
//
// Every outcome is a redirect: back to the page, which says what happened (ChangePasswordOutcome), or to the sign-in page.
app.MapPost(ChangePasswordOutcome.SubmitPath, async (
    SignInManager<WombatIdentityUser> signInManager,
    UserManager<WombatIdentityUser> userManager,
    IAuditWriter auditWriter,
    TimeProvider clock,
    IOptions<IdentityOptions> identityOptions,
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
            return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.SessionEnded, ChangePasswordOutcome.PagePath));
        }

        user = current;

        // Its password does not sign it in, and an SSO-provisioned account has none: a check could only count failures
        // towards a lockout, which would block its SSO sign-in too.
        if (!user.AllowLocalPassword)
        {
            return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.InstitutionalSignIn]));
        }

        // An account locked already, by someone else's guesses at the sign-in page, say: Identity would answer the check
        // below with a lockout without checking the password, and this session would be taken for the one that guessed.
        // It is refused here, and nothing changes: no password is read, the throttle counts nothing, and the session goes
        // on (PasswordCheckLockout; the review of the t339 branch).
        if (await PasswordCheckLockout.LockedForAsync(userManager, user, clock, identityOptions.Value.Lockout.DefaultLockoutTimeSpan)
            is { } lockedFor)
        {
            return Results.LocalRedirect(ChangePasswordOutcome.LockedUrl(lockedFor));
        }

        // The sign-in throttle, as the sign-in and link pages apply it: ten failed password checks in five minutes from one
        // address, shared with them. A refusal comes back to this page, which says why (T265, T156).
        var attempt = throttle.Begin(httpContext);
        if (attempt.Refused)
        {
            SignInThrottle.SetRetryAfter(httpContext.Response, attempt);
            return Results.LocalRedirect(ChangePasswordOutcome.RefusedUrl([ChangePasswordOutcome.TooManyAttempts]));
        }

        // The fifth wrong one locks the account.
        var check = await signInManager.CheckPasswordSignInAsync(user, request.CurrentPassword, lockoutOnFailure: true);
        if (check.Succeeded)
        {
            attempt.Release();
        }

        // The lock ends the session that guessed, as well as the sign-ins to come, and the sign-in page says why (T339,
        // flow 02, E2). Until T339 the page said the account was locked, and the session went on: a stolen cookie could wait
        // the lock out and guess five more. Signing this browser out would not stop that, since a copy of the cookie still
        // carries the account's stamp, so the stamp is changed: every session of the account ends at the stamp validator's
        // next look, as an administrator's lock ends them. The account was not locked before this check, so the lock is
        // this request's (PasswordCheckLockout).
        if (check.IsLockedOut)
        {
            await PasswordCheckLockout.EndSessionsAsync(
                httpContext, signInManager, userManager, auditWriter,
                loggerFactory.CreateLogger(ChangePasswordOutcome.LogCategory), user, PasswordCheckLockout.ChangePasswordAction);
            return Results.LocalRedirect(SignInOutcome.Url(ChangePasswordOutcome.LockedSignedOut));
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
        ActingRoleSwitchResults.Forget(httpContext);
        return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.PasswordChanged));
    }

    return Results.LocalRedirect(ChangePasswordOutcome.UpdatedUrl);
})
.RequireAuthorization();

// Where the sign-in page sends a user whose session had ended before they pressed Change password: the post was
// challenged, and the address it was going to comes back as the sign-in's return address, as a GET. Without this it
// answered 405 with an empty page (T265 review).
app.MapGet(ChangePasswordOutcome.SubmitPath, () => Results.LocalRedirect(ChangePasswordOutcome.PagePath))
    .RequireAuthorization();

// My account's form posts here (T335, flow 01; the review of the t335 branch). The shell names the person from the sign-in
// cookie's display_name claim, so the name is saved and the cookie issued again in one request, as change password does,
// and the browser comes back to the page by a full load, whose circuit starts from the new cookie. Saved in the page's
// circuit, the name left the claim stale there and in the cookie, and the account row went on showing the old name.
//
// The session is checked first, as change password checks it (T265 review): one the account no longer accepts is signed
// out, not issued a cookie. A refusal comes back as a code (ProfileOutcome), never the words.
app.MapPost(ProfileOutcome.SubmitPath, async (
    SignInManager<WombatIdentityUser> signInManager,
    ISender sender,
    ILoggerFactory loggerFactory,
    HttpContext httpContext,
    [FromForm] ProfileRequest request) =>
{
    var firstName = request.FirstName?.Trim() ?? string.Empty;
    var lastName = request.LastName?.Trim() ?? string.Empty;
    // The blank field by name, so the page marks it and keeps the other as stored (T339, flow 02); both blank, both.
    if (firstName.Length == 0 || lastName.Length == 0)
    {
        return Results.LocalRedirect(ProfileOutcome.RefusedUrl(
            firstName.Length == 0 && lastName.Length == 0 ? ProfileOutcome.NameMissing
            : firstName.Length == 0 ? ProfileOutcome.FirstNameMissing
            : ProfileOutcome.LastNameMissing));
    }

    if (firstName.Length > ProfileOutcome.MaxNameLength || lastName.Length > ProfileOutcome.MaxNameLength)
    {
        return Results.LocalRedirect(ProfileOutcome.RefusedUrl(ProfileOutcome.NameTooLong));
    }

    WombatIdentityUser account;
    try
    {
        var current = await signInManager.ValidateSecurityStampAsync(httpContext.User);
        if (current is null)
        {
            await signInManager.SignOutAsync();
            return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.SessionEnded, ProfileOutcome.PagePath));
        }

        // The signed-in user, and nobody else: the command reads the id from the caller (T185).
        await sender.Send(new UpdateCurrentUserProfileCommand(httpContext.User, firstName, lastName));
        account = current;
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        loggerFactory.CreateLogger(ProfileOutcome.LogCategory).LogError(exception, "A name could not be saved.");
        return Results.LocalRedirect(ProfileOutcome.RefusedUrl(ProfileOutcome.Failed));
    }

    // The cookie again, its display_name read from the account as saved (WombatUserClaimsPrincipalFactory), keeping the
    // old one's "remember me" and how the user signed in. The name is saved by now, so a fault here does not say it is
    // not: the cookie catches up at the stamp validator's next look (SessionRevalidation), within a minute.
    try
    {
        await signInManager.RefreshSignInAsync(account);
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
        loggerFactory.CreateLogger(ProfileOutcome.LogCategory)
            .LogError(exception, "A name was saved, but the sign-in cookie could not be issued again.");
    }

    return Results.LocalRedirect(ProfileOutcome.SavedUrl);
})
.RequireAuthorization();

// My account's Remove of an institutional sign-in (T339, flow 02): SignInMethodEndpoints.
app.MapSignInMethodEndpoints();

// Where a tab goes, by a full page load, once its circuit's sign-in has ended (SessionEnd, the T279 review): a session the
// account no longer accepts is signed out here, whatever the cookie's age, and sent to the sign-in page, which loads
// signed out and says the session has ended; a session the account still accepts goes back to the page it was on. Open to
// anyone, because by now the browser's cookie may already have been refused.
app.MapGet(SessionEnd.Path, (
    HttpContext httpContext,
    SignInManager<WombatIdentityUser> signInManager,
    ILoggerFactory loggerFactory,
    [FromQuery] string? returnUrl) => SessionEnd.HandleAsync(httpContext, signInManager, loggerFactory, returnUrl))
.AllowAnonymous();

// The sign-out page's form and the shell's Sign out post here (SignOutOutcome, T339, flow 02, B1). Not to the page's own
// address: a Razor component's address answers POST as well as GET, so the page could not also be /account/logout while
// the post was mapped there. Signing out lands on the sign-in page, which says so.
//
// The antiforgery token is required of a signed-in request (E6): without it a page that could send the sign-in cookie
// could sign a Wombat user out with a form of its own. A request that arrives not signed in is only redirected, to where
// signing out lands, and changes nothing: no sign-out, no cookie deleted. That is a tab whose session has ended already
// (a cookie the stamp validator refuses, it clears itself), whose token names someone the request no longer does; but it is
// also another site's auto-submitted form, since the sign-in cookie is SameSite=Lax and a browser does not send it on a
// cross-site POST. Until the review of the t339 branch this branch called SignOutAsync, whose Set-Cookie deletions the
// browser applies whether or not it sent the cookies: any site could sign a Wombat user out, and end a link in progress by
// deleting the external cookie, with no token at all.
app.MapPost(SignOutOutcome.SubmitPath, async (
    SignInManager<WombatIdentityUser> signInManager,
    IAuditWriter auditWriter,
    IAntiforgery antiforgery,
    HttpContext httpContext) =>
{
    if (!httpContext.User.Identities.Any(identity => identity.IsAuthenticated))
    {
        return Results.LocalRedirect(SignOutOutcome.SignedOutUrl);
    }

    if (!await antiforgery.IsRequestValidAsync(httpContext))
    {
        return Results.BadRequest();
    }

    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var display = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
    // Read BEFORE SignOutAsync — afterwards the principal is gone and the row would be unstamped,
    // i.e. Administrator-only under T101's audit scoping.
    var institutionId = httpContext.User.GetInstitutionId();

    await signInManager.SignOutAsync();

    // Nothing of this person's choices outlives their sign-out on this browser (T317): the acting role is the account's,
    // and a switch's word not yet shown goes with them.
    ActingRoleSwitchResults.Forget(httpContext);

    await auditWriter.WriteAsync(AuditEntry.Create(
        occurredAt: DateTime.UtcNow,
        category: AuditCategory.Authentication,
        action: "Logout",
        success: true,
        actorUserId: userId,
        actorDisplay: display,
        actorIpAddress: TruncateLoginIp(httpContext.Connection.RemoteIpAddress),
        institutionId: institutionId));

    return Results.LocalRedirect(SignOutOutcome.SignedOutUrl);
})
.AllowAnonymous();

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

// The acting role's switch, a GET so an email's link can use it (W-010): a held role is stored with the account and the
// cookie issued again; any other writes nothing; the return address is followed only when it is local (T335, flow 01).
app.MapGet(ActingRoleSwitch.Route, ActingRoleSwitch.HandleAsync)
    .RequireAuthorization();

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
static string? TruncateLoginIp(System.Net.IPAddress? address) => PasswordCheckLockout.AuditAddress(address);

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

/// <summary>My account's form (T335, flow 01). The account is the signed-in user's, never the form's.</summary>
internal sealed class ProfileRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
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
