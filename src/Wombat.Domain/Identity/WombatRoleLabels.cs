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
}
