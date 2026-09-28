using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Accounts;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Navigation;

namespace Wombat.Web.Security;

/// <summary>
/// My account's removal of an institutional sign-in (T339, flow 02; T286's removal; the round 2 review's A8, B-D6, E3).
/// </summary>
/// <remarks>
/// <para>
/// The Remove dialog is a real form the browser posts here (ConfirmDialog's form mode, C7), never an action in the page's
/// circuit: removing a login changes the account's security stamp, and only a request can issue the cookie again with the
/// new one. Every outcome is a redirect back to My account with a status or a code (<see cref="ProfileOutcome" />), or to
/// the sign-in page.
/// </para>
/// <para>
/// In order: the session is checked first, as the name and password endpoints check it (a session the account no longer
/// accepts is signed out, not issued a cookie); the account is the cookie's, never the form's, and the form names only the
/// provider; the last way in is refused before any password is read; an account locked already is refused, and nothing
/// changes (<see cref="PasswordCheckLockout" />); a password the account has is checked with lockout, under the sign-in
/// throttle, and the fifth wrong one ends the account's sessions (E2, E3); then the command, which checks the
/// last way in again against the account's concurrency stamp, so two tabs cannot remove two sign-ins past the guard; then
/// the cookie again. The form's antiforgery token is required: the endpoint binds a form.
/// </para>
/// </remarks>
public static class SignInMethodEndpoints
{
    /// <summary>Where the Remove dialog's form posts.</summary>
    public const string RemovePath = "/account/external-logins/remove";

    /// <summary>The sign-in page's notice for a session signed out by a lockout (E2, E3): change password's code.</summary>
    internal const string LockedSignedOutCode = ChangePasswordOutcome.LockedSignedOut;

    public static IEndpointRouteBuilder MapSignInMethodEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(SignInMethodEndpoints.RemovePath, RemoveAsync).RequireAuthorization();

        // Where the sign-in page sends a user whose session had ended before they pressed Remove: the post was challenged,
        // and its address comes back as the sign-in's return address, as a GET (as change password's does, T265 review).
        endpoints.MapGet(SignInMethodEndpoints.RemovePath, () => Results.LocalRedirect(ProfileOutcome.PagePath)).RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> RemoveAsync(
        SignInManager<WombatIdentityUser> signInManager,
        UserManager<WombatIdentityUser> userManager,
        ISender sender,
        IAuditWriter auditWriter,
        TimeProvider clock,
        IOptions<IdentityOptions> identityOptions,
        ILoggerFactory loggerFactory,
        SignInThrottle throttle,
        HttpContext httpContext,
        [FromForm] RemoveSignInRequest request)
    {
        var provider = request.Provider?.Trim() ?? string.Empty;
        if (provider.Length == 0)
        {
            return Results.LocalRedirect(ProfileOutcome.RefusedUrl(ProfileOutcome.RemoveFailed));
        }

        var refused = (string code) => Results.LocalRedirect(ProfileOutcome.RemoveRefusedUrl(code, provider));
        var logger = loggerFactory.CreateLogger(ProfileOutcome.LogCategory);

        WombatIdentityUser account;
        try
        {
            // The account, only if the cookie's stamp is still the account's (T265 review).
            var current = await signInManager.ValidateSecurityStampAsync(httpContext.User);
            if (current is null)
            {
                await signInManager.SignOutAsync();
                return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.SessionEnded, ProfileOutcome.PagePath));
            }

            account = current;

            var logins = await userManager.GetLoginsAsync(account);
            if (!logins.Any(login => string.Equals(login.LoginProvider, provider, StringComparison.Ordinal)))
            {
                // Never linked, or removed already by another tab.
                return refused(ProfileOutcome.RemoveFailed);
            }

            // The last way in, before any password is read: the page shows no live Remove for it, so only a crafted or a
            // second tab's post reaches this (R3-MA-RemoveStates e).
            var hasPassword = account.AllowLocalPassword && await userManager.HasPasswordAsync(account);
            if (!hasPassword && logins.Count <= 1)
            {
                return refused(ProfileOutcome.RemoveLastSignIn);
            }

            // E3: the account's password, when it has one, checked as the sign-in page checks it (T156, T265).
            if (hasPassword)
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return refused(ProfileOutcome.RemoveWrongPassword);
                }

                // Locked already, by someone else's guesses, say: Identity would answer the check with a lockout without
                // checking the password, and this session would be taken for the one that guessed. Refused in the dialog,
                // and nothing changes (the review of the t339 branch).
                if (await PasswordCheckLockout.LockedForAsync(
                        userManager, account, clock, identityOptions.Value.Lockout.DefaultLockoutTimeSpan) is { } lockedFor)
                {
                    return Results.LocalRedirect(ProfileOutcome.RemoveLockedUrl(provider, lockedFor));
                }

                var attempt = throttle.Begin(httpContext);
                if (attempt.Refused)
                {
                    SignInThrottle.SetRetryAfter(httpContext.Response, attempt);
                    return refused(ProfileOutcome.RemoveTooManyAttempts);
                }

                var check = await signInManager.CheckPasswordSignInAsync(account, request.Password, lockoutOnFailure: true);
                if (check.Succeeded)
                {
                    attempt.Release();
                }
                else if (check.IsLockedOut)
                {
                    // E2, as change password: this check locked the account, so the session that guessed is ended, and the
                    // sign-in page says why. Ended everywhere, by a new security stamp, not only in this browser: a copy of
                    // the cookie would otherwise go on working once the lockout lifts. Audited (PasswordCheckLockout).
                    await PasswordCheckLockout.EndSessionsAsync(
                        httpContext, signInManager, userManager, auditWriter, logger, account, PasswordCheckLockout.RemoveSignInAction);
                    return Results.LocalRedirect(SignInOutcome.Url(LockedSignedOutCode, null));
                }
                else
                {
                    return refused(check.IsNotAllowed ? ProfileOutcome.RemoveFailed : ProfileOutcome.RemoveWrongPassword);
                }
            }

            // The signed-in user's own sign-in, and nobody else's: the command reads the id from the caller.
            await sender.Send(new RemoveMyInstitutionalSignInCommand(httpContext.User, provider));
        }
        catch (InstitutionalSignInRemovalRefusedException refusal)
        {
            return refused(refusal.Reason == InstitutionalSignInRemoval.LastWayIn
                ? ProfileOutcome.RemoveLastSignIn
                : ProfileOutcome.RemoveFailed);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "An institutional sign-in could not be removed.");
            return refused(ProfileOutcome.RemoveFailed);
        }

        // The cookie again, with the stamp the removal changed, keeping the old one's "remember me". The sign-in is gone by
        // now, so a fault here must not say it is not: the old cookie's stamp is stale, so the session is ended here, and
        // the sign-in page asks the person to sign in again.
        try
        {
            await signInManager.RefreshSignInAsync(account);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "An institutional sign-in was removed, but the sign-in cookie could not be issued again.");
            await signInManager.SignOutAsync();
            ActingRoleSwitchResults.Forget(httpContext);
            return Results.LocalRedirect(SignInOutcome.Url(SignInOutcome.SessionEnded, ProfileOutcome.PagePath));
        }

        return Results.LocalRedirect(ProfileOutcome.SignInRemovedUrl(provider));
    }
}

/// <summary>
/// The Remove dialog's form: the provider, and the password when the account has one. The account is the signed-in
/// user's, and the login is found by the provider; no form names a subject or an account (T339).
/// </summary>
internal sealed class RemoveSignInRequest
{
    public string? Provider { get; init; }
    public string? Password { get; init; }
}
