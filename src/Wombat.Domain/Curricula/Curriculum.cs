namespace Wombat.Domain.Curricula;

public sealed class Curriculum
{
    public int Id { get; set; }
    public int SubSpecialityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Wombat.Domain.Institutions.SubSpeciality SubSpeciality { get; set; } = null!;
    public ICollection<CurriculumItem> Items { get; set; } = [];

    public Curriculum CloneAsNewVersion(string version, DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        return new Curriculum
        {
            SubSpecialityId = SubSpecialityId,
            Name = Name,
            Version = version.Trim(),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
            // Clone only the national core (OwningInstitutionId == null). Institution-local extras
            // belong to the institution, not the College's published version, and stay pinned to the
            // version they were added on; institutions re-add their extras after re-adopting (T091).
            Items = Items
                .Where(item => item.OwningInstitutionId is null)
                .OrderBy(item => item.Id)
                .Select(item => new CurriculumItem
                {
                    EpaId = item.EpaId,
                    RequiredCount = item.RequiredCount,
                    // Carried deliberately (T130): RequiredCount is a target PER this window, so cloning the
                    // number without its window would silently turn a per-semester target into a per-year one.
                    QuotaPeriod = item.QuotaPeriod,
                    MinimumLevelOrder = item.MinimumLevelOrder,
                    WindowMonths = item.WindowMonths,
                    Weight = item.Weight,
                    MinimumLevelByStageJson = item.MinimumLevelByStageJson,
                    // Carried deliberately: the cloned minima are the same numbers on the same ladder, so
                    // dropping the pin here would silently unpin every item of every new curriculum version
                    // and regenerate the T109 defect one version at a time.
                    ScaleId = item.ScaleId,
                    // Carried deliberately (T122): the tool list is the fourth cell of the same published row. A
                    // clone that dropped it would silently let every instrument credit every EPA again, one
                    // curriculum version at a time.
                    PermittedToolsJson = item.PermittedToolsJson,
                    // Carried deliberately (T131): the decision cadence, the body that decides and the opportunistic flag
                    // are the entrustment-decision cells of the same published row. A clone that dropped the cadence would
                    // leave every EPA of the new version never due; one that defaulted it would make every EPA due each
                    // academic year, the enum's zero value; one that dropped the body would route EPAs 4 and 5 to the
                    // general panel.
                    DecisionCadence = item.DecisionCadence,
                    DecisionBodyKey = item.DecisionBodyKey,
                    DecisionIsOpportunistic = item.DecisionIsOpportunistic
                })
                .ToList()
        };
    }
}
