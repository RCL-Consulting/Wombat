using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users;

public static class UserAdministrationRules
{
    /// <summary>
    /// Roles that may be added or removed via the admin Users surface. Excludes:
    /// - Administrator: must remain DB-direct (per CLAUDE.md, also cannot be assigned via SSO).
    /// - PendingTrainee: system-managed by the invitation acceptance pipeline.
    /// </summary>
    public static readonly IReadOnlyCollection<string> AssignableRoles =
    [
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.Assessor,
        WombatRoles.Trainee
    ];

    public static bool IsAssignableRole(string role) => AssignableRoles.Contains(role);
}
