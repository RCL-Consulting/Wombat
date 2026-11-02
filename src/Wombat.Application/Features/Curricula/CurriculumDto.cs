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

    /// <summary>
    /// The version's state for one institution (T392, flow 15, Q9, Q10, Q18, F6, S1, S3): the caller's own for an
    /// Institutional admin, the institution she chose with Show for the Administrator on Curricula, null otherwise (the
    /// College admin; the Administrator's All curricula and her items page). Filled by <c>GetCurriculaListQuery</c> and
    /// <c>GetCurriculumByIdQuery</c> (lane A, build-contracts § 1); null from every command's returned curriculum.
    /// </summary>
    public CurriculumInstitutionStateDto? Institution { get; init; }

    /// <summary>
    /// Whether the items page offers an add to this caller (T392, F6, S12): <see cref="CanEditCurriculum" /> (a national
    /// item), or an Institutional admin whose institution's adoption of this version is the active one (her own item). False
    /// on a version her institution has superseded: its own items keep Edit and Remove for the registrars still on it, and
    /// nothing is added. Whether an EPA is left to add is the picker's (<c>ListCurriculumItemEpaOptionsQuery</c>), not this.
    /// Filled by <c>GetCurriculumByIdQuery</c> (lane A); left at false, a projection that forgot it offers no add.
    /// </summary>
    public bool MayAddItem { get; init; }
}

/// <summary>
/// A curriculum version's state for one institution, in the words the Curricula rows and the items page's subtitle say
/// (T392, flow 15, Q10, F6, S1): "Adopted on ‹date›", or "Superseded on ‹date›" over "‹N› registrars still on it".
/// </summary>
/// <param name="InstitutionId">The institution.</param>
/// <param name="InstitutionName">Its name, for the Administrator's section ("Kgosi Kgari Teaching Hospital's own").</param>
/// <param name="IsActiveAdoption">Whether this version is the institution's active adoption for its discipline.</param>
/// <param name="AdoptedOn">The day of the institution's latest adoption of this version (the South African day, T325).</param>
/// <param name="SupersededOn">
/// For a superseded version, the day the adoption that followed its latest adoption was made, ordered by (AdoptedOn, Id)
/// (S1: both may be dated the same day, and going back adds a second row); null for the active one.
/// </param>
/// <param name="SupersededByName">For a superseded version, the name of the version that superseded it; else null.</param>
/// <param name="SupersededByVersion">Its version ("11.2"); else null.</param>
/// <param name="RegistrarCount">
/// The institution's ACTIVE trainee profiles on this version (<c>TraineeProfile.InstitutionId</c> and <c>CurriculumId</c>,
/// never the nullable <c>AdoptionId</c>; S1).
/// </param>
/// <param name="NationalItemCount">The version's national items.</param>
/// <param name="OwnItemCount">The institution's own items on this version.</param>
/// <param name="OwnItemsElsewhere">
/// The institution's own items on the other versions of this curriculum's discipline, by version, for the items page's
/// "A new version carries no institution's own items. Your item on 11.1 stays there." (R3-B07, B14); empty when none.
/// </param>
public sealed record CurriculumInstitutionStateDto(
    int InstitutionId,
    string InstitutionName,
    bool IsActiveAdoption,
    DateOnly AdoptedOn,
    DateOnly? SupersededOn,
    string? SupersededByName,
    string? SupersededByVersion,
    int RegistrarCount,
    int NationalItemCount,
    int OwnItemCount,
    IReadOnlyList<OwnItemsOnVersionDto> OwnItemsElsewhere);

/// <summary>An institution's own items on one other version of a curriculum (T392, R3-B07): "Your item on 11.1 stays there."</summary>
public sealed record OwnItemsOnVersionDto(int CurriculumId, string Version, int Count);

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
    /// The name of the institution whose own item this is, for a caller whose reads span institutions (T222); null for a
    /// national item, and for a caller who reads no institution's items but their own, whom the editor tells "Your
    /// institution's own item". Looked up by both item projections (<c>CurriculumRow.Projection</c>,
    /// <c>CurriculumMappings.ToDtoAsync</c>), since an item has no navigation to its institution, and cut to the caller by
    /// <see cref="CurriculumAdminScope.ForCaller" />. Positional and not defaulted, between two parameters of other types,
    /// so a projection that forgot it fails to compile.
    /// </summary>
    string? OwningInstitutionName,
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
