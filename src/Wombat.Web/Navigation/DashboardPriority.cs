using Wombat.Domain.Identity;

namespace Wombat.Web.Navigation;

public static class DashboardPriority
{
    /// <summary>
    /// Role priority order, highest first: the precedence the acting role falls back to (<see cref="ActingRoleResolver" />).
    /// A person who holds several roles acts as the first they hold, unless they have chosen another they hold. The choice
    /// is stored with the account (T335, flow 01, D1); until then it was a browser cookie, which outlived sign-out (T317).
    /// </summary>
    public static readonly IReadOnlyList<string> Order =
    [
        WombatRoles.Administrator,
        WombatRoles.CollegeAdmin,
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.CommitteeMember,
        WombatRoles.Coordinator,
        WombatRoles.Assessor,
        WombatRoles.Trainee,
        WombatRoles.PendingTrainee
    ];

    public static readonly IReadOnlySet<string> ValidRoles = new HashSet<string>(Order, StringComparer.Ordinal);
}
