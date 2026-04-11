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
            Items = Items
                .OrderBy(item => item.Id)
                .Select(item => new CurriculumItem
                {
                    EpaId = item.EpaId,
                    RequiredCount = item.RequiredCount,
                    MinimumLevelOrder = item.MinimumLevelOrder,
                    WindowMonths = item.WindowMonths,
                    Weight = item.Weight
                })
                .ToList()
        };
    }
}
