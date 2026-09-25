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

    /// <summary>
    /// The practice domain this EPA belongs to, as published by the owning College — for example
    /// the College of Paediatricians' five domains: "Ambulatory, Emergency and Critical Care in
    /// Children", "Care for the Newborn", "Chronic, Developmental and Preventive Care",
    /// "Leadership, Population and Systems Health", "Ethics, Communication, Education and
    /// Palliative Care". Null when the College publishes no domain grouping.
    /// </summary>
    /// <remarks>
    /// Deliberately a free-text label rather than an enum: domains are defined per College and
    /// differ between disciplines, so a fixed set would have to change whenever a new College
    /// adopts Wombat. Contrast <see cref="EpaCategory"/>, which is a Wombat-wide concept.
    /// </remarks>
    public string? Domain { get; set; }

    public string? Description { get; set; }
    public string? RequiredKnowledgeSkills { get; set; }
    public EpaCategory Category { get; set; } = EpaCategory.Core;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Wombat.Domain.Institutions.SubSpeciality SubSpeciality { get; set; } = null!;
    public ICollection<Wombat.Domain.Curricula.CurriculumItem> CurriculumItems { get; set; } = [];
}
