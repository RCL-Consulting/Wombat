using Microsoft.AspNetCore.Identity;
using Wombat.Infrastructure.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// Where a tab goes once its circuit's sign-in has ended (<see cref="SessionRevalidatingAuthenticationStateProvider" />):
/// <see cref="Path" />, loaded in full, which ends the session in the browser too and sends it to the sign-in page, or
/// back to the page it was on when the browser's own session is still good. (The T279 review.)
/// </summary>
/// <remarks>
/// <para>
/// A circuit whose sign-in ended used to go to the sign-in page inside the circuit. That page's form carried the
/// antiforgery token the circuit was given on its first page, which names the user who opened it. The browser still held
/// that user's cookie, which the stamp validator refused at the post, so the post was anonymous, the token named someone
/// else, and antiforgery answered 400 with an empty page: nobody the check signed out could sign back in without
/// reloading, and a locked user never saw why. So the tab leaves the circuit by a full page load
/// (<see cref="EndedSessionExit" />), to this endpoint.
/// </para>
/// <para>
/// A full load of the sign-in page alone would not be enough. The stamp validator looks at a cookie once a minute, so a
/// cookie renewed by another tab less than a minute before the change still signs the load in as the old user: the page
/// would be interactive, and its token would name them again. This endpoint asks the account itself, whatever the
/// cookie's age (<see cref="SignInManager{TUser}.ValidateSecurityStampAsync(System.Security.Claims.ClaimsPrincipal)" />):
/// a session that has ended, or one that cannot be checked, is signed out, and the sign-in page that follows loads
/// signed out, static, with a token of its own, and says the session has ended (<see cref="SignInOutcome.SessionEnded" />).
/// </para>
/// <para>
/// A cookie the account still accepts is left alone, and the browser goes back to where it was. That is a session other
/// than the circuit's: the user signed in again in another tab, or the circuit ended on faults alone. So a GET here never
/// signs a good session out, and a link to it cannot be used to.
/// </para>
/// </remarks>
public static class SessionEnd
{
    /// <summary>The endpoint.</summary>
    public const string Path = "/account/session-ended";

    /// <summary>The endpoint's log category.</summary>
    public const string LogCategory = "Wombat.Web.Account.SessionEnded";

    /// <summary>The endpoint, coming back to <paramref name="returnUrl" /> if it is a path on this site.</summary>
    public static string Url(string? returnUrl)
        => LocalOrNull(returnUrl) is { } local ? $"{Path}?returnUrl={Uri.EscapeDataString(local)}" : Path;

    /// <summary>
    /// <paramref name="url" /> when it is a path on this site, as <c>LocalRedirect</c> judges one: it starts with one slash,
    /// not two and not a slash and a backslash, and carries no control character. Null for anything else.
    /// </summary>
    public static string? LocalOrNull(string? url)
    {
        if (string.IsNullOrEmpty(url) || url[0] != '/')
        {
            return null;
        }

        if (url.Length > 1 && (url[1] == '/' || url[1] == '\\'))
        {
            return null;
        }

        return url.Any(char.IsControl) ? null : url;
    }

    /// <summary>
    /// The endpoint: back to <paramref name="returnUrl" /> for a session the account still accepts; otherwise signed out,
    /// and to the sign-in page, which says the session has ended and comes back to <paramref name="returnUrl" />.
    /// </summary>
    public static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        SignInManager<WombatIdentityUser> signInManager,
        ILoggerFactory loggerFactory,
        string? returnUrl)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(signInManager);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var local = LocalOrNull(returnUrl);

        if (httpContext.User.Identities.Any(identity => identity.IsAuthenticated))
        {
            try
            {
                if (await signInManager.ValidateSecurityStampAsync(httpContext.User) is not null)
                {
                    return Results.LocalRedirect(local ?? "/");
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // A session that cannot be checked is not passed: the circuit that sent the browser here had already
                // stopped trusting it.
                loggerFactory.CreateLogger(LogCategory)
                    .LogError(exception, "A session that had ended in its circuit could not be checked; it is signed out.");
            }
        }

        await signInManager.SignOutAsync();
        return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.SessionEnded, local));
    }
}
