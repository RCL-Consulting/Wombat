namespace Wombat.Web.Navigation;

/// <summary>
/// Which role a programme page reads as (T358, flow 06; D2, E4): the pages admit several roles and read by one, whose scope
/// the subtitle names.
/// </summary>
/// <remarks>
/// The acting role when the page admits it, so the page reads as the frame the person is working in. Otherwise the first
/// role the page admits that the person holds, in <see cref="DashboardPriority.Order" />: a Committee member acting as
/// Assessor who types the address reads it as Committee member, and a Speciality admin who also sits on the committee,
/// acting as Assessor, reads it as Speciality admin. Never the union of the roles held (E4).
/// </remarks>
public static class ProgrammeReadAs
{
    /// <summary>
    /// The role to read as, or null when the person holds none of <paramref name="admits" /> (the page's attribute then
    /// refuses them before this is asked).
    /// </summary>
    /// <param name="acting">The acting role and every role held, as <c>Routes</c> cascades them.</param>
    /// <param name="admits">The roles the page admits (<c>ProgrammeScope.RosterRoles</c> or <c>WaitingRoles</c>).</param>
    public static string? RoleFor(ActingRole acting, IReadOnlyList<string> admits)
    {
        ArgumentNullException.ThrowIfNull(acting);
        ArgumentNullException.ThrowIfNull(admits);

        if (acting.Role is { } role && admits.Contains(role, StringComparer.Ordinal))
        {
            return role;
        }

        return DashboardPriority.Order.FirstOrDefault(held => acting.Holds(held) && admits.Contains(held, StringComparer.Ordinal));
    }
}
