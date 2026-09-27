using System.Security.Claims;
using Wombat.Application.Common.Security;

namespace Wombat.Web.Navigation;

/// <summary>
/// The one place the acting role is worked out (T335, flow 01, D1). Everything that needs it (App.razor for the shell,
/// Home, the switch endpoint's "previous role") goes through here, never through a cookie or <c>IHttpContextAccessor</c>
/// inside a circuit.
/// </summary>
/// <remarks>
/// <list type="number">
/// <item>The role stored with the account (<see cref="WombatClaimTypes.ActingRole" />), when the person holds it.</item>
/// <item>Otherwise the first role they hold in <see cref="DashboardPriority.Order" />.</item>
/// <item>Otherwise none.</item>
/// </list>
/// A stored role the person no longer holds is ignored rather than cleared: it is only a preference, and the frame never
/// shows a role that is not held. Pure: it reads its arguments and nothing else.
/// </remarks>
public static class ActingRoleResolver
{
    /// <summary>The acting role from the stored choice and the roles held.</summary>
    public static ActingRole Resolve(string? stored, IEnumerable<string> heldRoles)
    {
        ArgumentNullException.ThrowIfNull(heldRoles);

        var held = heldRoles.ToHashSet(StringComparer.Ordinal);
        var ordered = DashboardPriority.Order.Where(held.Contains).ToList();
        if (ordered.Count == 0)
        {
            return ActingRole.None;
        }

        var role = stored is not null && ordered.Contains(stored, StringComparer.Ordinal) ? stored : ordered[0];
        return new ActingRole(role, ordered);
    }

    /// <summary>
    /// The acting role of a signed-in principal, from its claims alone: the stored choice is its
    /// <see cref="WombatClaimTypes.ActingRole" /> claim, and the roles held are its role claims. A visitor who has not
    /// signed in has none.
    /// </summary>
    public static ActingRole Resolve(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!user.Identities.Any(identity => identity.IsAuthenticated))
        {
            return ActingRole.None;
        }

        return Resolve(user.FindFirst(WombatClaimTypes.ActingRole)?.Value, DashboardPriority.Order.Where(user.IsInRole));
    }
}
