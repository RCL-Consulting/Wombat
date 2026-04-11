namespace Wombat.Application.Features.Institutions;

public sealed record SubSpecialityDto(
    int Id,
    int SpecialityId,
    string Name,
    string? Description,
    bool IsActive);
