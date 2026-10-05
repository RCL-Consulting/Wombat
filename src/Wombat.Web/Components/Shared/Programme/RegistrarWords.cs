using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Web.Components.Shared.Progress;

namespace Wombat.Web.Components.Shared.Programme;

/// <summary>
/// The words of the registrar page, <c>/programme/trainees/{ProfileId:int}</c> (T358, flow 06, lane A2; R2-Registrar r1–r8;
/// C2, C8, C9; review 8): its tab and subtitle, its sections' headings, errors and empties, its loading and page error,
/// and the ended record's words written for staff (round-3-check 2).
/// </summary>
/// <remarks>
/// <para>
/// Flow 05's words are the registrar's own ("your programme ended", <c>ProgressWords</c>, <c>QuotaText.ProgrammeEnded</c>).
/// On this page a member of staff reads someone else's record, so every sentence that would say "you" or "your" says the
/// registrar's name, or "the programme" (review 8). No pronoun names a person (round-3-check 1).
/// </para>
/// <para>Dates ISO (T325; flow 05's D1).</para>
/// </remarks>
public static class RegistrarWords
{
    /// <summary>
    /// The subtitle under the name: "Training year 1 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics"; an
    /// ended programme, "Training year 2 · Programme ended 2026-10-02 · …" (r6), or "Programme completed 2026-10-04 · …".
    /// </summary>
    public static string Subtitle(ProgrammeTraineeDto registrar)
    {
        ArgumentNullException.ThrowIfNull(registrar);

        var year = registrar.TrainingYear is int trainingYear
            ? $"Training year {trainingYear.ToString(CultureInfo.InvariantCulture)}"
            : "Programme not started";
        var period = registrar.Ended switch
        {
            null => registrar.CurrentSemesterName,
            { EndedOn: { } on, Completed: true } => $"Programme completed {QuotaText.Iso(on)}",
            { EndedOn: { } on } => $"Programme ended {QuotaText.Iso(on)}",
            _ => "Programme ended"
        };
        var where = string.IsNullOrEmpty(registrar.SubSpecialityName)
            ? registrar.InstitutionName
            : $"{registrar.InstitutionName}, {registrar.SubSpecialityName}";
        return $"{year} · {period} · {where}";
    }

    /// <summary>The tab's title: "Nomsa Mahlangu · Wombat".</summary>
    public static string Tab(string name) => $"{name} · Wombat";

    /// <summary>The page error's h1, when nothing loaded and so no name is known (r5).</summary>
    public const string ErrorHeading = "Programme trainee";

    /// <summary>The page's status while it loads (r8).</summary>
    public const string Loading = "Loading this registrar's progress.";

    /// <summary>The page's load error (r5): fixed words, never the exception (T329).</summary>
    public const string LoadFailed =
        "Could not load this registrar's progress. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The page error's strong part.</summary>
    public const string LoadFailedLead = "Could not load this registrar's progress.";

    /// <summary>What every error on the page says after its lead.</summary>
    public const string TryAgainRest = "Nothing has changed. Try again, or come back in a few minutes.";

    // ─── The sections, in C9's order ────────────────────────────────────────

    public const string PeriodHeading = "This period";
    public const string EpasHeading = "EPAs";
    public const string StandingHeading = "Entrustment against Annexure A";
    public const string TrajectoriesHeading = "Rating trajectories";
    public const string WaitingHeading = "Waiting for assessors";
    public const string ReviewsHeading = "Committee reviews";

    /// <summary>Each section's own load error's lead (r4); each ends <see cref="TryAgainRest" />.</summary>
    public const string PeriodFailedLead = "Could not load this period.";
    public const string EpasFailedLead = "Could not load the EPAs.";
    public const string StandingFailedLead = "Could not load the standing.";
    public const string TrajectoriesFailedLead = "Could not load the rating trajectories.";
    public const string WaitingFailedLead = "Could not load what waits for an assessor.";
    public const string ReviewsFailedLead = "Could not load the committee reviews.";

    public const string PeriodFailed = PeriodFailedLead + " " + TryAgainRest;
    public const string EpasFailed = EpasFailedLead + " " + TryAgainRest;
    public const string StandingFailed = StandingFailedLead + " " + TryAgainRest;
    public const string TrajectoriesFailed = TrajectoriesFailedLead + " " + TryAgainRest;
    public const string WaitingFailed = WaitingFailedLead + " " + TryAgainRest;
    public const string ReviewsFailed = ReviewsFailedLead + " " + TryAgainRest;

    /// <summary>
    /// This period's line (r1): "Semester 2, 2026 ends on 2026-11-30. Training year 1 sets the minimum level each encounter
    /// is judged against." Its first sentence is My progress's (<see cref="ProgressWords.EndsLine" />), December's included.
    /// </summary>
    public static string PeriodLine(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var ends = ProgressWords.EndsLine(summary);
        return summary.TraineeStage is int stage
            ? $"{ends} Training year {stage.ToString(CultureInfo.InvariantCulture)} sets the minimum level each encounter is judged against."
            : ends;
    }

    /// <summary>
    /// Rating trajectories with no rating in the academic year drawn (r2): "No ratings yet in the 2026 academic year."; an
    /// ended programme's, "No ratings in the 2026 academic year." (r6).
    /// </summary>
    public static string NoRatings((DateOnly From, DateOnly To) window, bool ended = false)
        => $"No ratings {(ended ? string.Empty : "yet ")}in the {window.From.Year.ToString(CultureInfo.InvariantCulture)} academic year.";

    /// <summary>Waiting for assessors with nothing waiting (r1): "Nothing of Nomsa Mahlangu's waits for an assessor."</summary>
    public static string NothingWaiting(string name) => $"Nothing of {Possessive(name)} waits for an assessor.";

    /// <summary>Committee reviews with none (r1): "No review is scheduled for Nomsa Mahlangu."</summary>
    public static string NoReview(string name) => $"No review is scheduled for {name}.";

    /// <summary>A review's link (r3, r6): "Pre-graduation review, 2026-10-04", by its type and the day it sits.</summary>
    public static string ReviewLink(CommitteeReviewListItemDto review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return $"{CommitteeDecisionWording.ReviewTypeLabel(review.ReviewType)}, {QuotaText.Iso(review.ScheduledOn)}";
    }

    /// <summary>The line under a review's link: "Semester 2, 2026 · Scheduled".</summary>
    public static string ReviewMeta(CommitteeReviewListItemDto review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return $"Semester {review.Semester.ToString(CultureInfo.InvariantCulture)}, " +
               $"{review.AcademicYear.ToString(CultureInfo.InvariantCulture)} · {review.StateLabel}";
    }

    // ─── The ended record, for staff (round-3-check 2; review 8) ────────────

    /// <summary>
    /// The notice over an ended registrar's record (r6): "Pieter du Plessis's programme ended on 2026-10-02, part-way
    /// through Semester 2, 2026. No target applies after that; what follows is the record as it stood then, read-only."
    /// A completion: "Lerato Molefe completed the programme on 2026-10-04, …". A programme ended before Wombat recorded the
    /// day: "Pieter du Plessis's programme has ended. Wombat did not record the day it ended, so the periods are shown up to
    /// 2026-10-04; what follows is the record, read-only."
    /// </summary>
    public static string EndedNotice(string name, ProgrammeEndDto ended, string semester)
    {
        ArgumentNullException.ThrowIfNull(ended);

        return ended switch
        {
            { EndedOn: { } on, Completed: true } =>
                $"{name} completed the programme on {QuotaText.Iso(on)}, part-way through {semester}. {AfterThat}",
            { EndedOn: { } on } =>
                $"{Possessive(name)} programme ended on {QuotaText.Iso(on)}, part-way through {semester}. {AfterThat}",
            _ => $"{Possessive(name)} programme has ended. Wombat did not record the day it ended, so the periods are shown " +
                 $"up to {QuotaText.Iso(ended.Today)}; what follows is the record, read-only."
        };
    }

    private const string AfterThat = "No target applies after that; what follows is the record as it stood then, read-only.";

    /// <summary>"when the programme ended": after the ended record's training year and minimum (My progress's "when your programme ended").</summary>
    public const string EndedWhen = "when the programme ended";

    /// <summary>The same, for a programme whose end day was not recorded, read today: "today" / "now", as My progress says.</summary>
    public static string EndedWhenOr(ProgrammeEndDto ended, string unrecorded)
    {
        ArgumentNullException.ThrowIfNull(ended);
        return ended.EndedOn is null ? unrecorded : EndedWhen;
    }

    /// <summary>The ended record's programme card's heading (My progress's "Your programme").</summary>
    public const string EndedProgrammeHeading = "The programme";

    /// <summary>An ended record whose every EPA has left use (My progress's "No EPA on your curriculum is in use any more…").</summary>
    public static string EndedNoItems(string name)
        => $"No EPA on {Possessive(name)} curriculum is in use any more, so there are no targets to show.";

    /// <summary>
    /// A count cell of an ended period with no target (Spec item 8): "No target (the programme ended part-way through)",
    /// with no figure and no bar.
    /// </summary>
    public const string EndedNoTarget = "No target (the programme ended part-way through)";

    /// <summary>
    /// One period of an ended programme, for staff: <see cref="ProgressWords.EndedPeriodLine" />'s words with "the
    /// programme" for "your programme" (review 8). "no target (the programme ended part-way through) · 1 recorded",
    /// "no target (started part-way through) · 0 recorded", "1 of 3, 2 short; 1 at the minimum level when observed".
    /// </summary>
    public static string EndedPeriodLine(QuotaWindowDto period, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(period);

        if (period.IsExempt)
        {
            return $"no target (started part-way through) · {period.Count} recorded";
        }

        if (period.EndedPartWay)
        {
            return $"no target (the programme ended part-way through) · {period.Count} recorded";
        }

        if (period.IsAfterProgrammeEnd)
        {
            return $"no target (after the programme ended) · {period.Count} recorded";
        }

        if (!period.Applies)
        {
            return "no target (before the programme started)";
        }

        // The counted case has no person in it: My progress's own words.
        return ProgressWords.EndedPeriodLine(period, today);
    }

    /// <summary>"Nomsa Mahlangu's", "Pieter du Plessis's" (as flow 03 builds a possessive).</summary>
    public static string Possessive(string name) => $"{name}'s";
}
