using System.Text.Json.Serialization;

namespace Wombat.Web.Navigation;

/// <summary>
/// The role whose frame the shell shows (its navigation, its landing, its head), and every role the person holds, in
/// <see cref="DashboardPriority.Order" />. Resolved once per request by <see cref="ActingRoleResolver" />, in
/// <c>App.razor</c>, passed to <c>Routes</c> and cascaded from there, fixed for the page or the circuit. (T335, flow 01)
/// </summary>
/// <remarks>
/// The acting role chooses what the frame shows, never what opens: access is the union of <see cref="HeldRoles" />, and
/// a page any of them admits opens whatever the acting role (R2-Rules § 1). Null for someone who holds no role, who has
/// no "Acting as" head and no switch (D8).
/// </remarks>
/// <param name="Role">The acting role's key, or null for someone who holds no role.</param>
/// <param name="HeldRoles">Every role held, in <see cref="DashboardPriority.Order" />, each once.</param>
public sealed record ActingRole(string? Role, IReadOnlyList<string> HeldRoles)
{
    /// <summary>A visitor who has not signed in, or someone who holds no role.</summary>
    public static readonly ActingRole None = new(null, []);

    /// <summary>The roles held beside the acting one, in <see cref="DashboardPriority.Order" />: what the switch offers.</summary>
    [JsonIgnore]
    public IReadOnlyList<string> OtherRoles
        => HeldRoles.Where(role => !string.Equals(role, Role, StringComparison.Ordinal)).ToList();

    /// <summary>Whether the person holds <paramref name="role" />.</summary>
    public bool Holds(string? role) => role is not null && HeldRoles.Contains(role, StringComparer.Ordinal);

    public bool Equals(ActingRole? other)
        => other is not null
            && string.Equals(Role, other.Role, StringComparison.Ordinal)
            && HeldRoles.SequenceEqual(other.HeldRoles, StringComparer.Ordinal);

    public override int GetHashCode() => HashCode.Combine(Role, HeldRoles.Count);
}

/// <summary>
/// What a switch through <see cref="ActingRoleSwitch" /> did, carried once to the page it lands on: the role the person
/// was acting as, when there was one, and the role they now act as. The page says so under its header
/// (<c>ActingRoleSwitchAlert</c>, R2-Rules § 1).
/// </summary>
/// <param name="Nonce">
/// The switch's own random word, from the cookie that carried it (<see cref="ActingRoleSwitchResults" />): the server
/// remembers the circuit that said it, so a circuit resumed from the page's descriptor, which is given it again, does not
/// say it again. Null where there is nothing to remember it by.
/// </param>
public sealed record ActingRoleSwitchResult(string? From, string To, string? Nonce = null);
