namespace Wombat.Application.Features.Institutions;

public sealed record SpecialityDto(
    int Id,
    int CollegeId,
    string Name,
    string? Description,
    bool IsActive);
