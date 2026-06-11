using Wombat.Domain.Identity;

namespace Wombat.Web.Navigation;

public static class DashboardPriority
{
    public const string CookieName = "wombat_preferred_dashboard_role";

    /// <summary>
    /// Role priority order, highest first. A user with multiple roles
    /// sees the dashboard for the first match unless overridden by cookie.
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
