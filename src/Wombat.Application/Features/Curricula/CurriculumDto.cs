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
    int? SubSpecialityDefaultScaleId)
{
    /// <summary>
    /// Whether the caller may change the curriculum itself: its details, a clone of it, and its national items. That is
    /// <c>CanAccessCollege</c>, the rule <c>UpdateCurriculumCommand</c>, <c>CloneCurriculumAsNewVersionCommand</c> and the
    /// national items' commands enforce (T211). Set only by <see cref="CurriculumAdminScope.ForCaller" />; left at false,
    /// a projection that forgot it offers nothing, which the handlers would refuse anyway.
    /// </summary>
    public bool CanEditCurriculum { get; init; }
}

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
    /// <summary>
    /// Whether the item's EPA is active, and so whether the item is in force (<see cref="CurriculumItemsInForce" />,
    /// T158). The editor lists every item as a record and marks one that is not in force: nobody can file against it,
    /// it takes no credit, and no progress page shows it. Positional and not defaulted for the reason above: a
    /// projection that forgot it would show a retired item as in force.
    /// </summary>
    bool EpaIsActive,
    /// <summary>
    /// The institution whose own item this is, or null for a national item of the College's (T091). Positional and not
    /// defaulted, and placed before a parameter of another type, so a projection that forgot it fails to compile: read
    /// as null, another institution's local item would pass for a national one, and be shown to every adopter (T211).
    /// </summary>
    int? OwningInstitutionId,
    /// <summary>
    /// How often a committee decides this EPA, or null for no published cadence (T131). Nullable and not defaulted:
    /// <c>AcademicYear</c> is the enum's zero value, so a projection that filled it in by default would make the EPA due
    /// every year, and an editor that re-saved the DTO would store it.
    /// </summary>
    Wombat.Domain.Curricula.QuotaPeriod? DecisionCadence,
    /// <summary>The key of the body that decides this EPA, or null for the general panel (T131).</summary>
    string? DecisionBodyKey,
    /// <summary>That body's name, for display; null with the key.</summary>
    string? DecisionBodyName,
    /// <summary>Whether the EPA is decided as opportunity allows, so its decision is never overdue (T131, O7).</summary>
    bool DecisionIsOpportunistic,
    /// <summary>The entrustment scale the minima above are expressed on, or null when unpinned (T109).</summary>
    int? ScaleId = null,
    string? ScaleName = null)
{
    /// <summary>The tool keys in <see cref="PermittedToolsJson" />, normalised and sorted. Empty means any instrument.</summary>
    public IReadOnlyList<string> PermittedToolKeys => Wombat.Domain.Curricula.CurriculumItem.ParsePermittedTools(PermittedToolsJson);

    /// <summary>Whether this is an institution's own item rather than one of the College's (T091).</summary>
    public bool IsLocal => OwningInstitutionId is not null;

    /// <summary>
    /// Whether the caller may edit and remove this item: <see cref="CurriculumItemEpas.MayWrite" />, the rule the Update and
    /// Remove commands enforce (T211). The editor offers Edit and Remove only where it is true. Set only by
    /// <see cref="CurriculumAdminScope.ForCaller" />; left at false, a projection that forgot it offers nothing.
    /// </summary>
    public bool CanEdit { get; init; }
}
