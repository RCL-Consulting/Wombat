using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// How an agenda line reads on the page: its stored state, told apart where the state alone does not say enough.
/// (T131 slice 4)
/// </summary>
public enum CommitteeAgendaLineStatus
{
    /// <summary>A closing line still to be decided: it must be staged or deferred before ratify.</summary>
    Due = 1,

    /// <summary>An annual line before the year's last sitting: optional here, and never missed (Decision 5).</summary>
    DueByYearEnd = 2,

    /// <summary>The trainee joined the window part-way through: optional, and never missed (Decision 9).</summary>
    PartialPeriod = 3,

    /// <summary>An EPA the College has decided as opportunity allows (O7), or a chair's line: optional.</summary>
    AsOpportunityAllows = 4,

    /// <summary>A decision is staged on it at this review.</summary>
    Staged = 5,

    /// <summary>Ratifying issued a STAR on it.</summary>
    Decided = 6,

    /// <summary>The chair put it off, with a reason.</summary>
    Deferred = 7,

    /// <summary>An optional line the review was ratified without.</summary>
    NotDecided = 8
}

/// <summary>One line of a review's agenda, as the page shows it. (T131 slice 4)</summary>
/// <param name="WindowLabel">"2026 S1" for a semester window, "2026" for an annual EPA's year.</param>
/// <param name="IsStaged">Whether a decision on the EPA is staged at the review: read, never stored.</param>
/// <param name="BlocksRatify">
/// Whether the line keeps the review from being ratified: <see cref="CommitteeAgendaLine.BlocksRatify" />, the predicate
/// the ratify handler enforces.
/// </param>
/// <param name="EvidenceCount">How many lines of the review's evidence snapshot are about the EPA.</param>
public sealed record CommitteeAgendaLineDto(
    int Id,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    CommitteeAgendaLineOrigin Origin,
    int WindowYear,
    int? WindowSemester,
    string WindowLabel,
    bool IsClosing,
    bool IsPartialPeriod,
    CommitteeAgendaLineState State,
    CommitteeAgendaLineStatus Status,
    bool IsStaged,
    bool BlocksRatify,
    string? DeferralReason,
    int? EntrustmentDecisionId,
    int EvidenceCount);

/// <summary>
/// Where the decision on an EPA another panel takes for the trainee stands in the window, read live. "Missed" is computed
/// here and never stored (Decision 6). (T131 slice 4)
/// </summary>
public enum CommitteeAgendaElsewhereStatus
{
    /// <summary>No sitting has it yet, and its window is still open (or it may not be missed).</summary>
    NotYetDecided = 1,

    /// <summary>It is on the agenda of an open review before that panel.</summary>
    OnAgenda = 2,

    /// <summary>That panel deferred it, with a reason, and nothing has decided it since.</summary>
    Deferred = 3,

    /// <summary>A STAR was issued on it in the window.</summary>
    Decided = 4,

    /// <summary>
    /// Its window has ended and nothing decided or deferred it, where it was not optional (not decided as opportunity
    /// allows, and not a partial period).
    /// </summary>
    Missed = 5
}

/// <summary>
/// An EPA due in the review's period that routing gives to another panel sitting as a College committee: shown read-only,
/// so a general sitting knows what the neonatal CCC still owes (the sitting order, O8). (T131 slice 4)
/// </summary>
public sealed record CommitteeAgendaElsewhereDto(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    int PanelId,
    string PanelName,
    string WindowLabel,
    CommitteeAgendaElsewhereStatus Status);

/// <summary>A review's agenda. (T131 slice 4)</summary>
/// <param name="PeriodLabel">The period the review sits for: "2026 S1".</param>
/// <param name="RoutedElsewhere">EPAs due in the period that another panel decides, read live.</param>
public sealed record CommitteeAgendaDto(
    int ReviewId,
    int AcademicYear,
    int Semester,
    string PeriodLabel,
    bool IsFormative,
    IReadOnlyList<CommitteeAgendaLineDto> Lines,
    IReadOnlyList<CommitteeAgendaElsewhereDto> RoutedElsewhere)
{
    /// <summary>The closing lines neither staged nor deferred, in code order: what keeps the review from being ratified.</summary>
    public IReadOnlyList<CommitteeAgendaLineDto> OutstandingClosingLines
        => Lines.Where(line => line.BlocksRatify)
            .OrderBy(line => line.EpaCode, StringComparer.Ordinal)
            .ThenBy(line => line.EpaId)
            .ToArray();

    /// <summary>
    /// Whether the review's decision records a progression category: false for an entrustment-only review (T131 slice 5),
    /// whose decision is what this agenda holds. Filled by the agenda's reader; true unless it says otherwise.
    /// </summary>
    public bool DecidesProgression { get; init; } = true;

    /// <summary>Why the review cannot be ratified yet, in the ratify refusal's own words; null when nothing blocks it.</summary>
    public string? RatifyBlockedReason
    {
        get
        {
            var outstanding = OutstandingClosingLines;
            return outstanding.Count == 0
                ? null
                : CommitteeReview.OutstandingClosingLinesReason(outstanding.Select(line => line.EpaCode).ToArray());
        }
    }

    /// <summary>
    /// Why the committee's decision cannot be recorded yet, in the record refusal's own words; null when the agenda does not
    /// block it. What blocks ratify first, since recording settles the agenda (slice 4); then, on an entrustment-only review,
    /// an empty agenda (<see cref="CommitteeReview.EmptyAgendaRefusal" />, slice 5), which ratify does not ask again.
    /// </summary>
    public string? RecordBlockedReason
        => RatifyBlockedReason
           ?? (!IsFormative && !DecidesProgression && Lines.Count == 0 ? CommitteeReview.NothingOnTheAgenda : null);

    /// <summary>The EPAs another panel takes that are not yet decided in their window.</summary>
    public IReadOnlyList<CommitteeAgendaElsewhereDto> UndecidedElsewhere
        => RoutedElsewhere.Where(line => line.Status != CommitteeAgendaElsewhereStatus.Decided).ToArray();
}

/// <summary>
/// What a review would put on its agenda, shown on the scheduling form before it is scheduled: the planner the
/// schedule and Start handlers use, so the preview is the agenda. (T131 slice 4)
/// </summary>
/// <param name="TraineeHasCurriculum">False when the trainee holds no profile, so nothing can be planned.</param>
/// <param name="DecidedInWindow">EPAs routed to this panel that a STAR already decided in their window: not planned.</param>
public sealed record CommitteeAgendaPreviewDto(
    int AcademicYear,
    int Semester,
    string PeriodLabel,
    bool TraineeHasCurriculum,
    IReadOnlyList<CommitteeAgendaLineDto> Lines,
    IReadOnlyList<CommitteeAgendaElsewhereDto> RoutedElsewhere,
    IReadOnlyList<string> DecidedInWindow)
{
    /// <summary>
    /// Whether the panel decides any EPA on the trainee's curriculum, due now or not (T131 slice 5): scheduling refuses an
    /// entrustment-only review before a panel that decides none, and the preview says so first. True unless the planner
    /// says otherwise.
    /// </summary>
    public bool PanelDecidesAnything { get; init; } = true;
}

/// <summary>The one reading of where a decision on an EPA stands in its window. (T131 slice 4)</summary>
public static class CommitteeAgendaStatus
{
    /// <summary>
    /// Where the decision on an EPA stands in its window, from the agenda lines about it in that window on any of the
    /// trainee's reviews: the one reading of "missed", computed and never stored (Decision 6).
    /// </summary>
    /// <param name="lines">
    /// Each line's state as it stands, and its review's state. The caller passes only what bears on the decision here: a
    /// line Decided on a STAR since revoked as NotDecided, and no line still due or deferred on a review at another
    /// institution (<see cref="AgendaPlanner" />).
    /// </param>
    /// <param name="mayBeMissed">
    /// False for an EPA decided as opportunity allows (O7) and for a window the trainee joined part-way through
    /// (Decision 9): neither is ever missed.
    /// </param>
    /// <param name="windowEnd">The window's last counted day: <see cref="AcademicPeriod.End" /> of its last semester.</param>
    public static CommitteeAgendaElsewhereStatus Elsewhere(
        IEnumerable<(CommitteeAgendaLineState Line, CommitteeReviewState Review)> lines,
        bool mayBeMissed,
        DateOnly windowEnd,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var states = lines.ToArray();

        if (states.Any(state => state.Line == CommitteeAgendaLineState.Decided))
        {
            return CommitteeAgendaElsewhereStatus.Decided;
        }

        if (states.Any(state => state.Line == CommitteeAgendaLineState.Due &&
                                state.Review is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress or CommitteeReviewState.Decided))
        {
            return CommitteeAgendaElsewhereStatus.OnAgenda;
        }

        if (states.Any(state => state.Line == CommitteeAgendaLineState.Deferred))
        {
            return CommitteeAgendaElsewhereStatus.Deferred;
        }

        return mayBeMissed && today > windowEnd
            ? CommitteeAgendaElsewhereStatus.Missed
            : CommitteeAgendaElsewhereStatus.NotYetDecided;
    }
}
