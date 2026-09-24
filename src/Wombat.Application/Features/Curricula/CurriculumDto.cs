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
    IReadOnlyList<CurriculumItemDto> Items,
    /// <summary>
    /// The sub-speciality's default entrustment scale, or null. Read only as the item editor's suggested ladder for
    /// a new item when the existing items do not already agree on one (T125); it never pins anything by itself, which
    /// is why <c>CurriculumItem.ScaleId</c> is not inferred from it.
    /// </summary>
    int? SubSpecialityDefaultScaleId);

public sealed record CurriculumItemDto(
    int Id,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    /// <summary>The target per <see cref="QuotaPeriod" /> window (T130, D18).</summary>
    int RequiredCount,
    Wombat.Domain.Curricula.QuotaPeriod QuotaPeriod,
    int MinimumLevelOrder,
    int WindowMonths,
    double? Weight,
    string? MinimumLevelByStageJson,
    /// <summary>
    /// The instruments that may credit this EPA, as the stored canonical JSON array of tool keys, or null for any
    /// instrument (T122). Positional and not defaulted, so a projection that forgot it fails to compile rather than
    /// hand the editor an item that reads as unrestricted. Read <see cref="PermittedToolKeys" /> for the parsed list.
    /// </summary>
    string? PermittedToolsJson,
    /// <summary>The entrustment scale the minima above are expressed on, or null when unpinned (T109).</summary>
    int? ScaleId = null,
    string? ScaleName = null)
{
    /// <summary>The tool keys in <see cref="PermittedToolsJson" />, normalised and sorted. Empty means any instrument.</summary>
    public IReadOnlyList<string> PermittedToolKeys => Wombat.Domain.Curricula.CurriculumItem.ParsePermittedTools(PermittedToolsJson);
}
