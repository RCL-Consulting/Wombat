namespace Wombat.Application.Features.Curricula;

public sealed record CurriculumDto(
    int Id,
    int SpecialityId,
    int SubSpecialityId,
    string SpecialityName,
    string SubSpecialityName,
    string CollegeName,
    string Name,
    string Version,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    bool CanEditInPlace,
    IReadOnlyList<CurriculumItemDto> Items);

public sealed record CurriculumItemDto(
    int Id,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    int RequiredCount,
    int MinimumLevelOrder,
    int WindowMonths,
    double? Weight,
    string? MinimumLevelByStageJson);
