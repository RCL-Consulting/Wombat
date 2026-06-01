namespace Wombat.Domain.Institutions;

public sealed class SubSpeciality
{
    public int Id { get; set; }
    public int SpecialityId { get; set; }
    public string Name { get; set; } = string.Empty;
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
    public ICollection<Wombat.Domain.Forms.AssessmentForm> AssessmentForms { get; set; } = [];
}
