using Microsoft.AspNetCore.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Security;

namespace Wombat.Web.Navigation;

/// <summary>
/// <c>GET /dashboard/switch/{role}?returnUrl=</c>: the one way the acting role changes (T335, flow 01; W-010; R2-Rules
/// § 1). The sidebar's "Switch to …" and "Change role" links, the result alert's "Switch back" and, from flow 19, the
/// emails' role-addressed links all use it.
/// </summary>
/// <remarks>
/// <para>
/// A GET, exempt from the rule that cookie and preference writes are form posts (W-010), because an email link cannot
/// post. What it writes is only a view preference: access is the union of the roles held and never changes, so a forged
/// link can at most change which frame someone who holds both roles sees.
/// </para>
/// <list type="bullet">
/// <item><b>A role the person holds:</b> stored with the account (<see cref="WombatIdentityUser.ActingRole" />, D1), the
/// sign-in cookie issued again so it carries the new choice, and a one-time word left for the page it lands on
/// (<see cref="ActingRoleSwitchResults" />).</item>
/// <item><b>An unknown role, or one not held:</b> nothing is written and nothing is said; the return address still
/// opens.</item>
/// <item><b>The return address</b> is followed only when it is a path on this site (<see cref="LocalUrl" />); anything
/// else, or none, lands on Home, which is the new role's landing.</item>
/// </list>
/// <para>
/// <b>Not audited.</b> The audit log records what grants or uses authority: sign-in, sign-out and every command. A switch
/// grants none and changes no record anyone else reads; the column holds the latest choice, which the person's access
/// report shows. An audit row per click would bury the rows that matter.
/// </para>
/// </remarks>
public static class ActingRoleSwitch
{
    /// <summary>The endpoint's route.</summary>
    public const string Route = "/dashboard/switch/{role}";

    /// <summary>The log category of a switch that could not be made.</summary>
    public const string LogCategory = "Wombat.Web.Navigation.ActingRoleSwitch";

    /// <summary>The switch to <paramref name="role" />, coming back to <paramref name="returnUrl" /> when it is a local path.</summary>
    public static string Url(string role, string? returnUrl = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(role);

        var path = $"/dashboard/switch/{Uri.EscapeDataString(role)}";
        return LocalUrl.OrNull(returnUrl) is { } local ? $"{path}?returnUrl={Uri.EscapeDataString(local)}" : path;
    }

    /// <summary>The endpoint.</summary>
    public static async Task<IResult> HandleAsync(
        string role,
        string? returnUrl,
        HttpContext httpContext,
        UserManager<WombatIdentityUser> userManager,
        SignInManager<WombatIdentityUser> signInManager,
        ActingRoleSwitchResults results,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        // Never LocalRedirect an address unchecked: it throws on one that is not local. A dropped one lands on Home.
        var destination = LocalUrl.OrNull(returnUrl) ?? "/";

        if (!DashboardPriority.ValidRoles.Contains(role))
        {
            return Results.LocalRedirect(destination);
        }

        try
        {
            // The account, only while the cookie's security stamp is still its own. A session an administrator's lock, a
            // change of roles or a password change elsewhere has ended is signed out here, not issued a cookie carrying
            // the current stamp, which would outlive the change (the change-password endpoint's rule, the T265 review).
            // The sign-in page comes back to this switch, so an email's link still does what it said.
            var account = await signInManager.ValidateSecurityStampAsync(httpContext.User);
            if (account is null)
            {
                await signInManager.SignOutAsync();
                return Results.LocalRedirect(SignInOutcome.Url(
                    SignInOutcome.SessionEnded,
                    httpContext.Request.Path + httpContext.Request.QueryString));
            }

            // Held as the account holds it now, not as the cookie remembers it.
            var held = await userManager.GetRolesAsync(account);
            if (!held.Contains(role, StringComparer.Ordinal))
            {
                return Results.LocalRedirect(destination);
            }

            var previous = ActingRoleResolver.Resolve(account.ActingRole, held).Role;

            if (!string.Equals(account.ActingRole, role, StringComparison.Ordinal))
            {
                account.ActingRole = role;
                var updated = await userManager.UpdateAsync(account);
                if (!updated.Succeeded)
                {
                    // A concurrent change to the account (its concurrency stamp): nothing was stored, so nothing is said.
                    loggerFactory.CreateLogger(LogCategory).LogWarning(
                        "An acting-role switch was not stored: {Errors}",
                        string.Join("; ", updated.Errors.Select(error => error.Code)));
                    return Results.LocalRedirect(destination);
                }
            }

            // The cookie again, carrying the new choice, with the old one's "remember me" and how the user signed in:
            // RefreshSignInAsync keeps the ticket's properties and its authentication-method claims. An institutional
            // (SSO) sign-in is issued with none (ExternalLoginHandler's SignInAsync), so it has none to lose.
            await signInManager.RefreshSignInAsync(account);

            // "You are now acting as …" only when the frame changes: a link naming the role already shown switches nothing.
            if (!string.Equals(previous, role, StringComparison.Ordinal))
            {
                results.Issue(httpContext, account.Id, previous, role);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The database gone mid-switch: the page still opens and says nothing. A choice stored before the fault reaches
            // the cookie at the stamp validator's next look (SessionRevalidation), which issues it from the account.
            loggerFactory.CreateLogger(LogCategory).LogError(exception, "An acting-role switch failed.");
        }

        return Results.LocalRedirect(destination);
    }
}
