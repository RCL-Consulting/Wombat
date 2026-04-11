namespace Wombat.Domain.Institutions;

public sealed class SubSpeciality
{
    public int Id { get; set; }
    public int SpecialityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Speciality Speciality { get; set; } = null!;
    public ICollection<Wombat.Domain.Epas.Epa> Epas { get; set; } = [];
    public ICollection<Wombat.Domain.Curricula.Curriculum> Curricula { get; set; } = [];
    public ICollection<Wombat.Domain.Forms.AssessmentForm> AssessmentForms { get; set; } = [];
}
