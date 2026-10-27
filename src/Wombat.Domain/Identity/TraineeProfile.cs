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

    /// <summary>
    /// The South African day the registrar was admitted to the programme (T358, D1): when admission wrote this profile, not
    /// <see cref="ProgrammeStartDate" />, which is the programme's start and is often months before or after it.
    /// </summary>
    /// <remarks>
    /// "Nothing filed in 30 days" counts from the later of this day and today − 30 (E5), so a registrar admitted this week is
    /// not listed for having filed nothing in a month they were not in Wombat for. Written by admission and the dev seed; the
    /// T358 migration set every profile it found to its <see cref="ProgrammeStartDate" />, the nearest day it held.
    /// </remarks>
    public DateOnly AdmittedOn { get; set; }

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

    /// <summary>
    /// Whether the programme has ended and its record is archived (T305): the profile is no longer active. Every way out of
    /// the programme (Mark complete, Deactivate, an erasure) clears <see cref="IsActive" />, and nothing sets it again.
    /// Mark complete says the profile is archived, so an ended profile is read-only: no save changes its curriculum, its
    /// adoption or its dates, which the trainee's progress, standing and portfolio are read from. The one rule: the
    /// profile's DTO and its page read it from <see cref="IsActive" /> alone too.
    /// </summary>
    public bool IsEnded => !IsActive;

    /// <summary>
    /// Whether <paramref name="day" /> is after the programme's last day (<see cref="EndedOn" />), and so outside it (T281).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The one rule for what an ended programme counts. An encounter observed after the last day credits nothing on this
    /// profile (<c>CreditApplier</c>, live and in every replay), and a multi-source feedback campaign that closed after it
    /// covers nothing on the profile's progress page. It is the end-side mirror of the programme-start bound (T160) and of
    /// D49, which puts the periods after the end outside the programme. The last day itself is inside it.
    /// </para>
    /// <para>
    /// False while the programme runs, and for a profile deactivated before Wombat recorded the day (T209): with no end
    /// recorded, nothing is after it. Total for any dates, as every reader of <see cref="EndedOn" /> is: an erased profile
    /// may record an end before its start (<see cref="Erase" />).
    /// </para>
    /// </remarks>
    public bool IsAfterEnd(DateOnly day) => IsAfterEnd(EndedOn, day);

    /// <inheritdoc cref="IsAfterEnd(DateOnly)" />
    /// <param name="endedOn">The profile's <see cref="EndedOn" />, for a reader that projected it rather than loading the profile.</param>
    /// <param name="day">The day judged: an encounter date, or the day a campaign closed.</param>
    public static bool IsAfterEnd(DateOnly? endedOn, DateOnly day) => endedOn is { } ended && day > ended;

    /// <summary>
    /// The list the programme ended with (T312), as stored (<see cref="EndedCurriculumItem.Serialize" />): every curriculum
    /// item in force for the trainee on their last day, national or their own institution's, with its target then. Null
    /// while the programme runs. Read it through <see cref="ReadEndedCurriculum" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Written once, when the end is recorded (<see cref="RecordEndedCurriculum" />), in the same save, and never again: the
    /// record of a closed programme does not move when the catalogue does. Before T312 the record read the live catalogue,
    /// so an EPA paused after a graduation left her record, and an item added after it gave closed years shortfalls she
    /// could never have met (F-6.21a, F-6.40a).
    /// </para>
    /// <para>
    /// Null also on an ended profile that no command ended (a test fixture that calls <see cref="Complete" /> alone). Its
    /// readers read the live catalogue then, as before T312. Every profile that had ended when T312 shipped was given its
    /// list by the T312 migration, from the catalogue as it stood.
    /// </para>
    /// </remarks>
    public string? EndedCurriculumJson { get; private set; }

    /// <summary>The list the programme ended with (<see cref="EndedCurriculumJson" />), or null when none is recorded.</summary>
    public IReadOnlyList<EndedCurriculumItem>? ReadEndedCurriculum() => EndedCurriculumItem.Parse(EndedCurriculumJson);

    /// <summary>
    /// Records the list the programme ended with (T312), once, after <see cref="Complete" />, <see cref="Deactivate" /> or
    /// <see cref="Erase" /> has ended it, and in the same save as the end.
    /// </summary>
    /// <param name="items">Every item in force for the trainee on the last day; empty when none was.</param>
    public void RecordEndedCurriculum(IEnumerable<EndedCurriculumItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (IsActive)
        {
            throw new InvalidOperationException("The list a programme ended with is recorded only once it has ended.");
        }

        if (EndedCurriculumJson is not null)
        {
            throw new InvalidOperationException("The list this programme ended with is already recorded.");
        }

        EndedCurriculumJson = EndedCurriculumItem.Serialize(items);
    }

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
        EnsureCanEnd(completedOn, today, "marked complete");

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
        EnsureCanEnd(deactivatedOn, today, "deactivated");

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
    /// and the day it ends on lies between the programme start and today (<see cref="EndDayRefusal" />).
    /// </summary>
    private void EnsureCanEnd(DateOnly endedOn, DateOnly today, string verb)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException($"Only an active trainee profile can be {verb}.");
        }

        if (EndDayRefusal(endedOn, ProgrammeStartDate, today) is { } refusal)
        {
            throw new InvalidOperationException(refusal);
        }
    }

    /// <summary>
    /// Why <paramref name="endedOn" /> cannot be recorded as a programme's last day, or null when it can: it must lie
    /// between the programme start and today on the South African calendar. One rule for both ends, in the words the profile
    /// page shows under its last-day field (T379, flow 13, F11 and S3): the page checks the day typed by it when Mark
    /// complete… or Deactivate… is pressed, before any dialog opens (Q21), and <see cref="Complete" /> and
    /// <see cref="Deactivate" /> check it again. Both refusals say "last day", for either end, and name the day that bounds
    /// it (until T379: "The completion date …", "The deactivation date …", and a before-start refusal that named no day).
    /// </summary>
    public static string? EndDayRefusal(DateOnly endedOn, DateOnly programmeStart, DateOnly today)
    {
        if (endedOn < programmeStart)
        {
            return $"The last day cannot be before the programme start date ({Iso(programmeStart)}).";
        }

        return endedOn > today ? $"The last day cannot be after today ({Iso(today)})." : null;
    }

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// The trainee's 1-based programme stage (year) on a given date, or null before the programme
    /// starts. Stage drives the effective minimum entrustment level per curriculum item
    /// (<see cref="CurriculumItem.GetMinimumLevelForStage"/>). Single source of truth shared by the
    /// trainee dashboard and the credit engine.
    /// </summary>
    public int? GetStage(DateOnly today) => StageOn(ProgrammeStartDate, today);

    /// <inheritdoc cref="GetStage(DateOnly)" />
    /// <param name="programmeStartDate">
    /// The programme start: a profile's, or the one a request is saving and has not saved yet (T304: the replay a move or a
    /// change of start runs judges each completion at the new start's training year).
    /// </param>
    /// <param name="day">The day whose stage is wanted.</param>
    public static int? StageOn(DateOnly programmeStartDate, DateOnly day)
    {
        if (day < programmeStartDate)
        {
            return null;
        }

        var daysElapsed = day.DayNumber - programmeStartDate.DayNumber;
        return (daysElapsed / 365) + 1;
    }
}
