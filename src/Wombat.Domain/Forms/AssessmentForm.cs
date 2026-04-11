namespace Wombat.Domain.Forms;

public sealed class AssessmentForm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? InstitutionId { get; set; }
    public int? SpecialityId { get; set; }
    public int? SubSpecialityId { get; set; }
    public int ScaleId { get; set; }
    public bool CanDelete { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public Wombat.Domain.Institutions.Institution? Institution { get; set; }
    public Wombat.Domain.Institutions.Speciality? Speciality { get; set; }
    public Wombat.Domain.Institutions.SubSpeciality? SubSpeciality { get; set; }
    public Wombat.Domain.Epas.EntrustmentScale Scale { get; set; } = null!;
    public ICollection<FormCriterion> Criteria { get; set; } = [];
    public ICollection<FormEpaLink> EpaLinks { get; set; } = [];
}
