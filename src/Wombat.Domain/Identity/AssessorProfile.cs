using Wombat.Domain.Institutions;

namespace Wombat.Domain.Identity;

public sealed class AssessorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;

    /// <summary>
    /// The assessor's institution, written from the person's own at every save (T376, flow 12, F4): erasure and the
    /// assessors list read it. The profile records training only; an assessor's discipline is the person's scope (Q12), so
    /// the profile's own speciality and sub-speciality, which nothing wrote after Q12 and nothing read, were dropped.
    /// </summary>
    public int InstitutionId { get; set; }
    public AssessorTrainingStatus TrainingStatus { get; set; } = AssessorTrainingStatus.NotStarted;
    public DateOnly? TrainingCompletedOn { get; set; }

    public Institution Institution { get; set; } = null!;
}
