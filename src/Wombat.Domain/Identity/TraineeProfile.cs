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

    /// <summary>
    /// The trainee's 1-based programme stage (year) on a given date, or null before the programme
    /// starts. Stage drives the effective minimum entrustment level per curriculum item
    /// (<see cref="CurriculumItem.GetMinimumLevelForStage"/>). Single source of truth shared by the
    /// trainee dashboard and the credit engine.
    /// </summary>
    public int? GetStage(DateOnly today)
    {
        if (today < ProgrammeStartDate)
        {
            return null;
        }

        var daysElapsed = today.DayNumber - ProgrammeStartDate.DayNumber;
        return (daysElapsed / 365) + 1;
    }
}
