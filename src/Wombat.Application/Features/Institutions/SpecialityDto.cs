namespace Wombat.Application.Features.Institutions;

public sealed record SpecialityDto(
    int Id,
    int InstitutionId,
    string Name,
    string? Description,
    bool IsActive);
