namespace Wombat.Domain.Identity;

/// <summary>
/// Maps an external IdP group to a Wombat role for a specific SSO provider.
/// Managed by administrators; applied on each SSO login by the group mapper.
/// </summary>
public sealed class SsoGroupRoleMapping
{
    public int Id { get; set; }

    /// <summary>
    /// The provider key from the Sso:Providers config (e.g. "uct").
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// The external group identifier (e.g. Entra object ID).
    /// </summary>
    public string ExternalGroupId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for admin UX (e.g. "Surgery Department").
    /// </summary>
    public string ExternalGroupDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The Wombat role to assign (from WombatRoles).
    /// </summary>
    public string WombatRole { get; set; } = string.Empty;

    /// <summary>
    /// Institution to scope the user to when this mapping fires.
    /// </summary>
    public int InstitutionId { get; set; }

    /// <summary>
    /// Optional speciality scope. If set, the user also gets a speciality scope record.
    /// </summary>
    public int? SpecialityId { get; set; }

    /// <summary>
    /// Optional sub-speciality scope.
    /// </summary>
    public int? SubSpecialityId { get; set; }
}
