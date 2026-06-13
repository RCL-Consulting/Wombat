namespace Wombat.Domain.Epas;

public sealed class Epa
{
    public int Id { get; set; }
    public int SubSpecialityId { get; set; }

    /// <summary>
    /// Null for a national (College-owned) EPA — the catalogue core. Set to an institution id for an
    /// institution-local supplementary EPA: institutions may add local extras to a discipline but can never
    /// edit the national core (T091, phase 3).
    /// </summary>
    public int? OwningInstitutionId { get; set; }
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
