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

    /// <summary>
    /// The date the trainee completed (graduated) the programme, or null. Distinguishes a graduated
    /// trainee from one that was merely deactivated/withdrawn (both have <see cref="IsActive"/> false).
    /// Set via <see cref="Complete"/> at the final committee review. (T080 / F-5-4)
    /// </summary>
    public DateOnly? CompletedOn { get; private set; }

    public Curriculum Curriculum { get; set; } = null!;

    /// <summary>
    /// Marks the programme complete (graduation): records the completion date and deactivates the
    /// profile. The caller is responsible for the role transition (removing the Trainee role).
    /// </summary>
    public void Complete(DateOnly completedOn)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Only an active trainee profile can be marked complete.");
        }

        if (completedOn < ProgrammeStartDate)
        {
            throw new InvalidOperationException("The completion date cannot be before the programme start date.");
        }

        CompletedOn = completedOn;
        IsActive = false;
    }

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
