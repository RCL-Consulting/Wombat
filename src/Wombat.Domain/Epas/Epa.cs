namespace Wombat.Domain.Epas;

public sealed class Epa
{
    public int Id { get; set; }
    public int SubSpecialityId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequiredKnowledgeSkills { get; set; }
    public EpaCategory Category { get; set; } = EpaCategory.Core;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Wombat.Domain.Institutions.SubSpeciality SubSpeciality { get; set; } = null!;
    public ICollection<Wombat.Domain.Curricula.CurriculumItem> CurriculumItems { get; set; } = [];
    public ICollection<Wombat.Domain.Forms.FormEpaLink> FormLinks { get; set; } = [];
}
