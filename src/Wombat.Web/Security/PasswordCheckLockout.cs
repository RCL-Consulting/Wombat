using Microsoft.AspNetCore.Identity;
using Wombat.Application.Audit;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Web.Navigation;

namespace Wombat.Web.Security;

/// <summary>
/// The lockout of a signed-in password check: change password's current password and the Remove dialog's password (T339,
/// flow 02, E2 and E3; the review of the t339 branch).
/// </summary>
/// <remarks>
/// <para>
/// <b>Whose lockout.</b> Identity answers <c>CheckPasswordSignInAsync</c> for an account that is locked already with
/// <c>LockedOut</c>, without checking the password. So a lockout the check reports may be a stranger's: five wrong
/// passwords at the sign-in page lock the account, and until the review of the t339 branch the owner's next change of
/// password, or Remove, was then taken for the fifth guess. It changed the stamp, ended every session of the account, and
/// told the owner they had typed their password wrong too many times. Each endpoint now reads the lock first
/// (<see cref="LockedForAsync" />): one that was there already is refused on the page, in words of its own, and nothing
/// else changes. Only a lock this request's check trips ends the sessions (<see cref="EndSessionsAsync" />). A lock that
/// another request trips between the two reads is taken for this one's: a few milliseconds, and the account is locked
/// either way.
/// </para>
/// <para>
/// <b>Ending the sessions.</b> The stamp is changed, so every copy of the cookie is refused at the stamp validator's next
/// look; a refused change is tried once more on the account read again, then logged, and this browser is signed out
/// whatever came of it. The acting role's one-time word goes with the sign-out (<see cref="ActingRoleSwitchResults.Forget" />),
/// and an audit row records the lockout, as the sign-in page's <c>LoginLockedOut</c> and the link page's
/// <c>SsoAccountLinkLockedOut</c> do: stamped with the account's institution, and never carrying the password.
/// </para>
/// </remarks>
internal static class PasswordCheckLockout
{
    /// <summary>The audit action for a lockout change password's check tripped.</summary>
    public const string ChangePasswordAction = "ChangePasswordLockedOut";

    /// <summary>The audit action for a lockout the Remove dialog's check tripped.</summary>
    public const string RemoveSignInAction = "RemoveSignInLockedOut";

    /// <summary>
    /// How long <paramref name="user" />'s lock has left, when the account is locked already; null when it is not. Never
    /// more than <paramref name="lockout" />, the lockout Identity is configured with: a lock longer than that is an
    /// administrator's, whose session has ended at the stamp check before this is read.
    /// </summary>
    public static async Task<TimeSpan?> LockedForAsync(
        UserManager<WombatIdentityUser> users,
        WombatIdentityUser user,
        TimeProvider clock,
        TimeSpan lockout)
    {
        if (!await users.IsLockedOutAsync(user))
        {
            return null;
        }

        var left = await users.GetLockoutEndDateAsync(user) is { } end ? end - clock.GetUtcNow() : lockout;
        return left <= TimeSpan.Zero || left > lockout ? lockout : left;
    }

    /// <summary>
    /// Ends every session of <paramref name="user" />, whose password check this request has just locked, and records it.
    /// Throws only if signing this browser out does.
    /// </summary>
    public static async Task EndSessionsAsync(
        HttpContext httpContext,
        SignInManager<WombatIdentityUser> signInManager,
        UserManager<WombatIdentityUser> users,
        IAuditWriter auditWriter,
        ILogger logger,
        WombatIdentityUser user,
        string action)
    {
        await ChangeStampAsync(httpContext, users, logger, user);

        await signInManager.SignOutAsync();

        // Nothing of this person's choices outlives their sign-out on this browser (T317), as at the sign-out page.
        ActingRoleSwitchResults.Forget(httpContext);

        try
        {
            await auditWriter.WriteAsync(AuditEntry.Create(
                occurredAt: DateTime.UtcNow,
                category: AuditCategory.Authentication,
                action: action,
                success: false,
                actorUserId: user.Id,
                actorDisplay: $"{user.FirstName} {user.LastName}".Trim(),
                actorIpAddress: AuditAddress(httpContext.Connection.RemoteIpAddress),
                actorUserAgent: httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } agent ? agent : null,
                institutionId: user.InstitutionId,
                errorMessage: "Account locked after repeated failed password checks; its sessions were ended."));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The sessions have ended; a missing row must not undo that, or tell the person something else happened.
            logger.LogError(exception, "The lockout of user {UserId} could not be written to the audit log.", user.Id);
        }
    }

    /// <summary>
    /// A sign-in audit row's address: an IPv4 address by its /24, an IPv6 one by its /48, as the sign-in page records it
    /// (T101). Program.cs's endpoints use it too.
    /// </summary>
    public static string? AuditAddress(System.Net.IPAddress? address)
    {
        if (address is null)
        {
            return null;
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            bytes[3] = 0;
            return new System.Net.IPAddress(bytes) + "/24";
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            for (var i = 6; i < 16; i++)
            {
                bytes[i] = 0;
            }

            return new System.Net.IPAddress(bytes) + "/48";
        }

        return address.ToString();
    }

    private static async Task ChangeStampAsync(
        HttpContext httpContext,
        UserManager<WombatIdentityUser> users,
        ILogger logger,
        WombatIdentityUser user)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                var stamped = await users.UpdateSecurityStampAsync(user);
                if (stamped.Succeeded)
                {
                    return;
                }

                logger.LogWarning(
                    "The security stamp of user {UserId}, locked by a password check, could not be changed (try {Attempt}): {Errors}",
                    user.Id, attempt, string.Join("; ", stamped.Errors.Select(error => error.Code)));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception,
                    "The security stamp of user {UserId}, locked by a password check, could not be changed (try {Attempt}).",
                    user.Id, attempt);
            }

            // Read again, so the second try, and any later save in this request, does not carry the refused stamp. A change
            // made elsewhere since the account was read (ConcurrencyFailure) is the usual cause.
            try
            {
                await httpContext.RequestServices.GetRequiredService<ApplicationDbContext>().Entry(user).ReloadAsync();
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "User {UserId} could not be read again after a refused change of stamp.", user.Id);
            }
        }

        logger.LogError(
            "User {UserId} was locked by a password check, but its security stamp could not be changed: its other sessions " +
            "go on until the stamp next changes. This browser is signed out.",
            user.Id);
    }
}
