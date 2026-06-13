using Wombat.Domain.Curricula;

namespace Wombat.Domain.Identity;

public sealed class TraineeProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The institution where the trainee trains. Held directly (mirrors AssessorProfile) because the
    /// curriculum is now a national, College-owned catalogue and no longer carries an institution —
    /// see T091. The trainee follows the national <see cref="Curriculum"/> version their institution
    /// adopted.
    /// </summary>
    public int InstitutionId { get; set; }
    public int CurriculumId { get; set; }

    /// <summary>
    /// The institution's adoption record that pins this trainee to the national curriculum version
    /// they follow (see <see cref="Wombat.Domain.Institutions.InstitutionCurriculumAdoption"/>).
    /// Nullable for profiles created before T091 phase 4; new admissions always set it. The pinned
    /// version equals <see cref="CurriculumId"/> at admission time even though the institution may
    /// later re-adopt a newer version for future trainees.
    /// </summary>
    public int? AdoptionId { get; set; }
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
