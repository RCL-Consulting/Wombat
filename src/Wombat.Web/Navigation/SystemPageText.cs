using System.Security.Claims;
using Wombat.Domain.Identity;

namespace Wombat.Web.Navigation;

/// <summary>
/// The words of the system pages (Access denied, the error page) that depend on who is signed in, from the principal's
/// claims alone (T335, flow 01; R2-Denied-In, R2-Error-In; the round-2 review, S1 and D6). A system page reads no database:
/// it is what a person sees when something else has gone wrong.
/// </summary>
public static class SystemPageText
{
    /// <summary>
    /// Why Access denied refused, in terms of the roles held and never of the page or the roles that would open it (D6):
    /// "Your role (Trainee) does not open this page." or "None of your roles (Committee member, Assessor) opens this page."
    /// </summary>
    /// <remarks>
    /// Access is the union of the roles held, so no switch of the acting role could open the page, and none is offered
    /// (S1). The roles are named in <see cref="DashboardPriority.Order" />, by label.
    /// </remarks>
    public static string Refusal(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var held = ActingRoleResolver.Resolve(user).HeldRoles;
        return held.Count switch
        {
            0 => "Your account holds no role that opens this page.",
            1 => $"Your role ({WombatRoleLabels.For(held[0])}) does not open this page.",
            _ => $"None of your roles ({string.Join(", ", held.Select(WombatRoleLabels.For))}) opens this page."
        };
    }

    /// <summary>
    /// Whom a signed-in person turns to, about a page they cannot open or a failure that keeps happening: "the platform
    /// administrator" for anyone who administers an institution, a College or the platform, since their institution's
    /// administrator would be themselves or nobody (S1); "your institution's Wombat administrator" for everyone else.
    /// </summary>
    public static string WhomToAsk(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return user.IsInRole(WombatRoles.Administrator)
            || user.IsInRole(WombatRoles.InstitutionalAdmin)
            || user.IsInRole(WombatRoles.CollegeAdmin)
                ? "the platform administrator"
                : "your institution's Wombat administrator";
    }

    /// <summary>Whether a principal has signed in: any identity authenticated, as the fallback policy reads it.</summary>
    public static bool IsSignedIn(ClaimsPrincipal? user)
        => user?.Identities.Any(identity => identity.IsAuthenticated) == true;
}
