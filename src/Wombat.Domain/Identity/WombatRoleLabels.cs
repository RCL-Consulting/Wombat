namespace Wombat.Domain.Identity;

/// <summary>
/// What a person reads for each role: a sentence-case label, never the key (<see cref="WombatRoles" />). "Committee
/// member", not "CommitteeMember". The shell's role head and switch, Home and the nominee gate's refusals all print these,
/// so a role is named the same way wherever it appears (T335, flow 01; T190's mixed capitalisation).
/// </summary>
public static class WombatRoleLabels
{
    /// <summary>Every role's label, by its key.</summary>
    public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [WombatRoles.Administrator] = "Administrator",
        [WombatRoles.CollegeAdmin] = "College admin",
        [WombatRoles.InstitutionalAdmin] = "Institutional admin",
        [WombatRoles.SpecialityAdmin] = "Speciality admin",
        [WombatRoles.SubSpecialityAdmin] = "Sub-speciality admin",
        [WombatRoles.Coordinator] = "Coordinator",
        [WombatRoles.CommitteeMember] = "Committee member",
        [WombatRoles.Assessor] = "Assessor",
        [WombatRoles.Trainee] = "Trainee",
        [WombatRoles.PendingTrainee] = "Pending trainee"
    };

    /// <summary>
    /// The role's label. A key that names no role is returned as it is: whether it is a role at all is the caller's
    /// question, and a name shown as typed is better than one silently dropped.
    /// </summary>
    public static string For(string role)
    {
        ArgumentNullException.ThrowIfNull(role);
        return All.TryGetValue(role, out var label) ? label : role;
    }

    /// <summary>
    /// The roles held, in <see cref="WombatRoles.All" />'s order, each once: one order on every page that lists a person's
    /// roles (T376, flow 12: Users' Roles cell, the user page's Roles and scope; build-contracts § 0). A key that names no
    /// role sorts last, ordinally. Keys, not labels: a page prints each through <see cref="For" />.
    /// </summary>
    public static IReadOnlyList<string> InOrder(IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        return roles
            .Distinct(StringComparer.Ordinal)
            .OrderBy(Rank)
            .ThenBy(role => role, StringComparer.Ordinal)
            .ToArray();
    }

    private static int Rank(string role)
    {
        for (var index = 0; index < WombatRoles.All.Count; index++)
        {
            if (string.Equals(WombatRoles.All[index], role, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return int.MaxValue;
    }
}
