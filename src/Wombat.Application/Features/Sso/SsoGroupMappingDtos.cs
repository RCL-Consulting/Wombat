namespace Wombat.Application.Features.Sso;

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
    string? SubSpecialityName);
