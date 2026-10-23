namespace Wombat.Application.Features.Sso;

/// <param name="ProviderName">The provider's display name, or its key when it is no longer set up (T376).</param>
/// <param name="ProviderGone">
/// The provider is no longer set up for the mapping's institution, so the mapping gives no new role; roles it already gave
/// remain. Read only to everyone but the Administrator, who alone may delete it (T376, S5).
/// </param>
public sealed record SsoGroupMappingDto(
    int Id,
    string ProviderKey,
    string ExternalGroupId,
    string ExternalGroupDisplayName,
    string WombatRole,
    int InstitutionId,
    string? InstitutionName,
    int? SpecialityId,
    string? SpecialityName,
    int? SubSpecialityId,
    string? SubSpecialityName,
    string? ProviderName = null,
    bool ProviderGone = false);
