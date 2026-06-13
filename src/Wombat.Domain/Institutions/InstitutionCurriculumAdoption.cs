using Wombat.Domain.Curricula;

namespace Wombat.Domain.Institutions;

/// <summary>
/// Records that a training institution has adopted a specific national (College-owned)
/// <see cref="Curriculum"/> version for one discipline (sub-speciality). Adoption is explicit and
/// version-pinned: the College publishes new curriculum versions and institutions opt into them by
/// re-adopting. Trainees are admitted only into the version their institution has adopted
/// (<see cref="Wombat.Domain.Identity.TraineeProfile.AdoptionId"/>). At most one adoption per
/// (institution, sub-speciality) may be active at a time. (T091 phase 4.)
/// </summary>
public sealed class InstitutionCurriculumAdoption
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }

    /// <summary>The pinned national curriculum version this institution has adopted.</summary>
    public int CurriculumId { get; set; }

    /// <summary>
    /// The discipline the adoption is for, denormalised from the adopted curriculum so the
    /// "one active adoption per (institution, discipline)" invariant can be enforced by a partial
    /// unique index without a join. Always equals <c>Curriculum.SubSpecialityId</c>.
    /// </summary>
    public int SubSpecialityId { get; set; }

    public DateOnly AdoptedOn { get; set; }
    public bool IsActive { get; set; } = true;

    public Institution Institution { get; set; } = null!;
    public Curriculum Curriculum { get; set; } = null!;
}
