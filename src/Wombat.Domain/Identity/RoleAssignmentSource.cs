namespace Wombat.Domain.Identity;

/// <summary>
/// Tracks how a role was assigned to a user, so SSO group sync
/// can distinguish its own assignments from manual ones.
/// </summary>
public enum RoleAssignmentSource
{
    Manual = 0,
    Sso = 1
}
