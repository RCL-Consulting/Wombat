using Wombat.Domain.Curricula;

namespace Wombat.Domain.Identity;

public sealed class TraineeProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CurriculumId { get; set; }
    public DateOnly ProgrammeStartDate { get; set; }
    public DateOnly ExpectedCompletionDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Curriculum Curriculum { get; set; } = null!;
}
