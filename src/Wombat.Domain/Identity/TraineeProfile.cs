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

    /// <summary>
    /// The trainee's last day in the programme when it was ended without being completed (a withdrawal), or null.
    /// Set via <see cref="Deactivate"/> (T209). Null on a profile deactivated before T209, whose day was never
    /// recorded, and on a completed one, whose day is <see cref="CompletedOn"/>.
    /// </summary>
    public DateOnly? DeactivatedOn { get; private set; }

    /// <summary>
    /// The day the programme actually ended: completion (<see cref="CompletedOn"/>) or deactivation
    /// (<see cref="DeactivatedOn"/>). Null while it is running, and for a profile deactivated before the day was recorded.
    /// Not <see cref="ExpectedCompletionDate"/>, which is a plan: a trainee still active after it is still in the
    /// programme. The quota reads it for D49: the period it falls in is exempt unless it falls in that period's last month,
    /// and periods after it are outside the programme (<see cref="QuotaWindow"/>).
    /// </summary>
    public DateOnly? EndedOn => CompletedOn ?? DeactivatedOn;

    public Curriculum Curriculum { get; set; } = null!;

    /// <summary>
    /// Marks the programme complete (graduation): records the completion date and deactivates the
    /// profile. The caller is responsible for the role transition (removing the Trainee role).
    /// </summary>
    /// <param name="completedOn">The graduation day: on or after the programme start, and not after <paramref name="today" />.</param>
    /// <param name="today">
    /// Today on the South African calendar. The profile ends now, so a later day would be an end that has not happened:
    /// D49 would read the periods up to it as still in the programme (T209 review). Every check runs before anything changes.
    /// </param>
    public void Complete(DateOnly completedOn, DateOnly today)
    {
        EnsureCanEnd(completedOn, today, "marked complete", "completion");

        CompletedOn = completedOn;
        IsActive = false;
    }

    /// <summary>
    /// Ends the programme without completing it (a withdrawal): records the trainee's last day and deactivates the
    /// profile (T209). The day is what D49 reads, so it is the day the trainee left, which an administrator may record
    /// after the fact, as a completion is, but never ahead of it. Every check runs before anything changes.
    /// </summary>
    /// <param name="deactivatedOn">The trainee's last day: on or after the programme start, and not after <paramref name="today" />.</param>
    /// <param name="today">
    /// Today on the South African calendar. The profile ends now, so a later day (a resignation's notice date, say) would
    /// be an end that has not happened: D49 would hold the trainee to every period up to it (T209 review).
    /// </param>
    public void Deactivate(DateOnly deactivatedOn, DateOnly today)
    {
        EnsureCanEnd(deactivatedOn, today, "deactivated", "deactivation");

        DeactivatedOn = deactivatedOn;
        IsActive = false;
    }

    /// <summary>
    /// Hands the profile to an erased person's pseudonym and ends it on the erasure day (T258). An erasure is the person
    /// leaving: a profile still active is deactivated as a withdrawal is, with <paramref name="erasedOn" /> as its last day
    /// (<see cref="DeactivatedOn" />, which D49 reads). A profile that had already ended keeps the end it recorded, completion
    /// or withdrawal: that is settled.
    /// </summary>
    /// <remarks>
    /// Never refuses, unlike <see cref="Deactivate" />: an erasure is the data subject's right and is not held to the
    /// programme's dates. So a profile whose programme had not yet begun records the erasure day as its end even though it
    /// is before the start; every reader of <see cref="EndedOn" /> is total for any dates (<c>QuotaWindow.For</c>).
    /// </remarks>
    /// <param name="pseudonym">The erased person's pseudonym, which no account holds.</param>
    /// <param name="erasedOn">The erasure day on the South African calendar.</param>
    public void Erase(string pseudonym, DateOnly erasedOn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pseudonym);

        UserId = pseudonym;
        if (IsActive)
        {
            DeactivatedOn = erasedOn;
            IsActive = false;
        }
    }

    /// <summary>
    /// The checks both ways out of the programme share, run before either changes anything: the profile is still active,
    /// and the day it ends on lies between the programme start and today.
    /// </summary>
    private void EnsureCanEnd(DateOnly endedOn, DateOnly today, string verb, string noun)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException($"Only an active trainee profile can be {verb}.");
        }

        if (endedOn < ProgrammeStartDate)
        {
            throw new InvalidOperationException($"The {noun} date cannot be before the programme start date.");
        }

        if (endedOn > today)
        {
            throw new InvalidOperationException($"The {noun} date cannot be after today ({today:yyyy-MM-dd}).");
        }
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
