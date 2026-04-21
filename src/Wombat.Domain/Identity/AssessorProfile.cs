using Wombat.Domain.Institutions;

namespace Wombat.Domain.Identity;

public sealed class AssessorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;
    public int InstitutionId { get; set; }
    public int? SpecialityId { get; set; }
    public int? SubSpecialityId { get; set; }
    public DateOnly? TrainingCompletedOn { get; set; }

    public Institution Institution { get; set; } = null!;
    public Speciality? Speciality { get; set; }
    public SubSpeciality? SubSpeciality { get; set; }
}
