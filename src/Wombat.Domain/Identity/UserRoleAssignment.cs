namespace Wombat.Domain.Identity;

/// <summary>
/// Tracks the source of each role assignment so the SSO group mapper
/// can remove roles it granted without touching manually-assigned ones.
/// </summary>
public sealed class UserRoleAssignment
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public RoleAssignmentSource Source { get; set; }

    /// <summary>
    /// For SSO-assigned roles, the provider key that granted it.
    /// </summary>
    public string? ProviderKey { get; set; }

    public DateTime AssignedOn { get; set; }
}
