namespace Wombat.Domain.Institutions;

public sealed class SubSpeciality
{
    public int Id { get; set; }
    public int SpecialityId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The key a seeder finds this row by, such as <c>cpsa:paediatrics:paediatrics</c>, or null for a row an administrator
    /// made (T221). Written when a seeder creates the row and never changed after: no command writes it, so neither a
    /// rename nor a move to another speciality can lose it.
    /// </summary>
    public string? SeedKey { get; init; }

    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The entrustment scale this programme grants authorisation on. When set, committee STARs for
    /// this sub-speciality's trainees are constrained to this scale's levels (the level picker filters
    /// to it). Null falls back to offering every scale. (T076 / F-4D-1)
    /// </summary>
    public int? DefaultEntrustmentScaleId { get; set; }

    public Speciality Speciality { get; set; } = null!;
    public Wombat.Domain.Epas.EntrustmentScale? DefaultEntrustmentScale { get; set; }
    public ICollection<Wombat.Domain.Epas.Epa> Epas { get; set; } = [];
    public ICollection<Wombat.Domain.Curricula.Curriculum> Curricula { get; set; } = [];
}
