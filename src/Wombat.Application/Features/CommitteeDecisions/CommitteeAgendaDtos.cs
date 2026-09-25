using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

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
    NotDecided = 8,

    /// <summary>
    /// Another sitting has decided the EPA in the line's window (T235): optional here, and never blocks ratify. Read live
    /// while the line is still due on an open review, by the one predicate the planner and the decisions-due page share
    /// (<see cref="CommitteeAgendaStatus.IsDecided" />), and stored once the review settles its agenda
    /// (<see cref="CommitteeAgendaLineState.DecidedElsewhere" />).
    /// </summary>
    DecidedElsewhere = 9
}

/// <summary>One line of a review's agenda, as the page shows it. (T131 slice 4)</summary>
/// <param name="WindowLabel">"2026 S1" for a semester window, "2026" for an annual EPA's year.</param>
/// <param name="IsStaged">Whether a decision on the EPA is staged at the review: read, never stored.</param>
/// <param name="BlocksRatify">
/// Whether the line keeps the review from being ratified: <see cref="CommitteeAgendaLine.BlocksRatify" />, the predicate
/// the record and ratify handlers enforce, with whether another sitting has decided its window read as they read it (T235).
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

/// <summary>An EPA named beside a review's agenda, off it: its id, so the chair can stage it, and its code and title. (T235)</summary>
public sealed record CommitteeAgendaEpaDto(int EpaId, string EpaCode, string EpaTitle);

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

    /// <summary>
    /// The codes of the EPAs routed to this panel and due for the period that a STAR already decided in their window, so
    /// the planner left them off (<see cref="CommitteeAgendaStatus.IsDecided" />, T215), in code order: the scheduling
    /// preview's <see cref="CommitteeAgendaPreviewDto.DecidedInWindow" />, read live. An EPA this review holds a line for is
    /// not named: its line says what this sitting did with it, and the decisions-due page where the decision stands now.
    /// Empty for a formative review.
    /// </summary>
    public IReadOnlyList<string> DecidedInWindow { get; init; } = [];

    /// <summary>
    /// On a review in progress, the EPAs routed to this panel and due for the period whose window was decided and has lost
    /// that decision (<see cref="CommitteeAgendaStatus.HasLostItsDecision" />, T235), and which the agenda does not hold, in
    /// code order: an EPA Start left off because a STAR decided it, whose STAR has since been revoked. Named so the chair
    /// can stage it, which adds the chair's line; never added by itself, since the agenda was planned at Start and the
    /// decision recorded at this sitting settles it (D46). Empty in every other state: before Start, Start plans the EPA;
    /// once the decision is recorded, nothing more is staged, and the decisions-due page reads it as revoked.
    /// </summary>
    public IReadOnlyList<CommitteeAgendaEpaDto> NoLongerDecided { get; init; } = [];

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
/// <param name="DecidedInWindow">
/// The codes of the EPAs routed to this panel that a STAR already decided in their window, whether or not an agenda line
/// records it (<see cref="CommitteeAgendaStatus.IsDecided" />, T215): not planned.
/// </param>
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

/// <summary>
/// When a committee review sat, in sitting order: the period it sat for, then the order reviews were scheduled in.
/// (T131 slice 6 review)
/// </summary>
/// <remarks>
/// What sittings did to one decision is read in this order: a deferral stands only while no later sitting in the window
/// has decided the EPA since, whatever has become of that decision. A sitting for a later period sat later. Within a
/// period, a later review was scheduled later, and one seat holds one open binding review per period, so a remediation
/// sitting comes after the sitting it remedies.
/// </remarks>
public readonly record struct CommitteeSitting(int AcademicYear, int Semester, int ReviewId)
{
    /// <summary>Whether this sitting came after <paramref name="other" />.</summary>
    public bool IsAfter(CommitteeSitting other) => Key.CompareTo(other.Key) > 0;

    /// <summary>The sitting order as one comparable value.</summary>
    public (int AcademicYear, int Semester, int ReviewId) Key => (AcademicYear, Semester, ReviewId);
}

/// <summary>A STAR as the window rule reads it: the sitting that issued it, and what has become of it. (T131 slice 6 review)</summary>
/// <param name="SupersededById">The STAR that superseded it, when it was superseded.</param>
/// <param name="Sitting">The sitting of the review that issued it.</param>
public sealed record DecisionWindowStar(
    int Id,
    string TraineeUserId,
    int EpaId,
    EntrustmentDecisionStatus Status,
    int? SupersededById,
    CommitteeSitting Sitting);

/// <summary>An agenda line about an EPA in its window, as where the decision stands reads it. (T131 slice 6 review)</summary>
/// <param name="State">The line's stored state.</param>
/// <param name="Review">Its review's state.</param>
/// <param name="Sitting">When its review sat.</param>
/// <param name="StarDecides">
/// For a Decided line, whether its STAR still decides the window (<see cref="CommitteeAgendaStatus.StarDecides" />).
/// </param>
public readonly record struct AgendaLineStanding(
    CommitteeAgendaLineState State,
    CommitteeReviewState Review,
    CommitteeSitting Sitting,
    bool StarDecides)
{
    /// <summary>Decided, on a STAR that still decides the window.</summary>
    public bool Decides => State == CommitteeAgendaLineState.Decided && StarDecides;

    /// <summary>Decided, on a STAR that no longer decides the window: a decision the window has lost.</summary>
    public bool Lapsed => State == CommitteeAgendaLineState.Decided && !StarDecides;
}

/// <summary>A STAR from a sitting in the window, as where the decision stands reads it. (T131 slice 6 review)</summary>
/// <param name="Decides">Whether it still decides the window (<see cref="CommitteeAgendaStatus.StarDecides" />).</param>
/// <param name="Sitting">The sitting of the review that issued it.</param>
public readonly record struct StarStanding(bool Decides, CommitteeSitting Sitting);

/// <summary>The one reading of where a decision on an EPA stands in its window. (T131 slice 4)</summary>
public static class CommitteeAgendaStatus
{
    /// <summary>
    /// Whether the EPA's window is decided: a STAR from a sitting for a period in the window still decides it
    /// (<see cref="StarDecides" />), whether or not an agenda line records it, or a line Decided on such a STAR does. The
    /// one predicate the agenda planner, its report of what another panel decides, and the decisions-due page share, so
    /// none of them can call due a window another calls decided (T215); and the one an open review's own lines are read
    /// against, when its agenda is read, its decision recorded and it is ratified, so a line whose window another sitting
    /// decided never blocks ratify (T235).
    /// </summary>
    /// <remarks>
    /// Agenda lines are the record of what a sitting did; the STAR is the fact. A STAR ratified before agendas existed, or
    /// issued by any other path, has no line and still decides its window. A revoked one decides nothing, and nor does one
    /// superseded inside the window by a STAR since revoked.
    /// </remarks>
    /// <param name="lines">The agenda lines about the EPA in the window, on any of the trainee's reviews.</param>
    /// <param name="starsInWindow">Every STAR on the EPA from a sitting for a period in the window.</param>
    public static bool IsDecided(IEnumerable<AgendaLineStanding> lines, IEnumerable<StarStanding> starsInWindow)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(starsInWindow);
        return starsInWindow.Any(star => star.Decides) || lines.Any(line => line.Decides);
    }

    /// <summary>
    /// Whether the EPA's window was decided and has lost that decision: <see cref="IsDecided" /> says no, and a STAR from a
    /// sitting in the window, or a line Decided on one, says it was. What the decisions-due page reads as "Revoked:
    /// re-decide", and what a review in progress names beside its agenda when the EPA is not on it (T235).
    /// </summary>
    /// <remarks>
    /// Nothing deciding the window, every STAR of it is one the window has lost: revoked, or superseded inside the window by
    /// one since revoked.
    /// </remarks>
    /// <param name="lines">The agenda lines about the EPA in the window, on any of the trainee's reviews.</param>
    /// <param name="starsInWindow">Every STAR on the EPA from a sitting for a period in the window.</param>
    public static bool HasLostItsDecision(IEnumerable<AgendaLineStanding> lines, IEnumerable<StarStanding> starsInWindow)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(starsInWindow);

        var lineStandings = lines.ToArray();
        var stars = starsInWindow.ToArray();
        return !IsDecided(lineStandings, stars) && (stars.Length > 0 || lineStandings.Any(line => line.Lapsed));
    }

    /// <summary>
    /// Where the decision on an EPA stands in its window, from the STARs of the window and the agenda lines about it in
    /// that window on any of the trainee's reviews: the one reading of "missed", computed and never stored (Decision 6).
    /// </summary>
    /// <remarks>
    /// <see cref="IsDecided" /> says Decided. Otherwise a line still due on an open review puts it on the agenda. A
    /// deferral stands only when it is the latest thing a sitting did about the EPA in the window: a later sitting that
    /// decided it, even on a STAR since revoked, has overtaken the deferral, and a sitting that deferred it after a
    /// decision was revoked is the latest word (T131 slice 6 review). Then Missed, once the window has ended.
    /// </remarks>
    /// <param name="lines">
    /// The lines. The caller passes only what bears on the decision here: no line still due or deferred on a review at
    /// another institution (<see cref="AgendaPlanner" />).
    /// </param>
    /// <param name="starsInWindow">
    /// Every STAR on the EPA from a sitting for a period in the window, whether or not a line records it: a STAR goes with
    /// the trainee.
    /// </param>
    /// <param name="mayBeMissed">
    /// False for an EPA decided as opportunity allows (O7) and for a window the trainee joined part-way through
    /// (Decision 9): neither is ever missed.
    /// </param>
    /// <param name="windowEnd">The window's last counted day: <see cref="AcademicPeriod.End" /> of its last semester.</param>
    public static CommitteeAgendaElsewhereStatus Elsewhere(
        IEnumerable<AgendaLineStanding> lines,
        IEnumerable<StarStanding> starsInWindow,
        bool mayBeMissed,
        DateOnly windowEnd,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(starsInWindow);

        var lineStandings = lines.ToArray();
        var stars = starsInWindow.ToArray();
        if (IsDecided(lineStandings, stars))
        {
            return CommitteeAgendaElsewhereStatus.Decided;
        }

        // Nothing decides the window, so every STAR of it is one the window has lost: revoked, or superseded inside it by
        // one since revoked. Each is a later decision, weighed against a deferral as a lapsed line is.
        return Standing(lineStandings, stars.Select(star => star.Sitting).ToArray(), mayBeMissed, windowEnd, today);
    }

    /// <summary>
    /// Where an undecided window stands (<see cref="Elsewhere" />), with <paramref name="lapses" />, the sittings of the
    /// window's lost decisions, weighed against a deferral as a lapsed line is.
    /// </summary>
    private static CommitteeAgendaElsewhereStatus Standing(
        AgendaLineStanding[] lines,
        IReadOnlyCollection<CommitteeSitting> lapses,
        bool mayBeMissed,
        DateOnly windowEnd,
        DateOnly today)
    {
        if (lines.Any(line => line.State == CommitteeAgendaLineState.Due && IsOpen(line.Review)))
        {
            return CommitteeAgendaElsewhereStatus.OnAgenda;
        }

        var deferrals = lines.Where(line => line.State == CommitteeAgendaLineState.Deferred).ToArray();
        if (deferrals.Length > 0)
        {
            var latestDeferral = deferrals.MaxBy(line => line.Sitting.Key).Sitting;
            var overtaken = lines.Where(line => line.Lapsed).Select(line => line.Sitting)
                .Concat(lapses)
                .Any(lapse => lapse.IsAfter(latestDeferral));

            if (!overtaken)
            {
                return CommitteeAgendaElsewhereStatus.Deferred;
            }
        }

        return mayBeMissed && today > windowEnd
            ? CommitteeAgendaElsewhereStatus.Missed
            : CommitteeAgendaElsewhereStatus.NotYetDecided;
    }

    /// <summary>Scheduled, in progress, or decided and awaiting ratify: a review that can still decide what it holds.</summary>
    private static bool IsOpen(CommitteeReviewState state)
        => state is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress or CommitteeReviewState.Decided;

    /// <summary>
    /// Where the decision on an EPA due for <paramref name="period" /> stands for one trainee, as the decisions-due page
    /// shows it (T131 slice 6): <see cref="Elsewhere" />'s reading of the agenda lines, with the STARs of the window taken
    /// as the fact of a decision, and "still to be decided" told apart by why.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Decided</b> is <see cref="IsDecided" />, the planner's rule: a STAR from a sitting for a period in the window
    /// that still decides it (<see cref="StarDecides" />), whether or not an agenda line records it. Agenda lines are the
    /// record of what a sitting did, and a STAR issued before agendas existed, or by any other path, still decides its
    /// window (T215).
    /// </para>
    /// <para>
    /// Otherwise <see cref="Elsewhere" /> reads the rest: on the agenda of an open review is <b>Scheduled</b>, then
    /// <b>Deferred</b>, where no later sitting has decided it since, the window's STARs counted with its lines. Then a
    /// window whose decision was revoked reads so ("re-decide") ahead of <b>Missed</b>, which is
    /// <see cref="Elsewhere" />'s one computation. What is left is still to be decided and not scheduled, and says why it
    /// cannot be missed where it cannot: a partial period (Decision 9), an EPA decided as opportunity allows (O7), or an
    /// annual EPA before its year's last semester (Decision 5).
    /// </para>
    /// </remarks>
    /// <param name="lines">
    /// The agenda lines about the EPA in the window, as for <see cref="Elsewhere" />: no line still due or deferred on a
    /// review before a panel at another institution.
    /// </param>
    /// <param name="starsInWindow">Every STAR on the EPA from a sitting for a period in the window.</param>
    /// <param name="isOpportunistic">Whether the College decides the EPA as opportunity allows (O7).</param>
    /// <param name="window">
    /// The EPA's cadence window holding <paramref name="period" />, judged for the trainee; it must not be
    /// <see cref="QuotaWindowStatus.NotStarted" /> (<see cref="CommitteeAgendaLine.IsDueAt" />).
    /// </param>
    /// <param name="period">The semester the page asks about.</param>
    /// <param name="today">The programme's today (<see cref="ProgrammeCalendar.DateOf" />): only "missed" reads it.</param>
    public static EntrustmentDecisionDueStatus DecisionDue(
        IEnumerable<AgendaLineStanding> lines,
        IEnumerable<StarStanding> starsInWindow,
        bool isOpportunistic,
        QuotaWindow window,
        AcademicPeriod period,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(starsInWindow);
        ArgumentNullException.ThrowIfNull(window);

        var stars = starsInWindow.ToArray();
        var lineStandings = lines.ToArray();
        var partial = window.Status == QuotaWindowStatus.ExemptPartialPeriod;
        var standing = Elsewhere(lineStandings, stars, mayBeMissed: !isOpportunistic && !partial, window.End, today);

        switch (standing)
        {
            case CommitteeAgendaElsewhereStatus.Decided:
                return EntrustmentDecisionDueStatus.Decided;
            case CommitteeAgendaElsewhereStatus.OnAgenda:
                return EntrustmentDecisionDueStatus.Scheduled;
            case CommitteeAgendaElsewhereStatus.Deferred:
                return EntrustmentDecisionDueStatus.Deferred;
        }

        // Nothing decides the window, so any STAR of it is one the window has lost: revoked, or superseded inside it by
        // one since revoked. The predicate a review in progress names such an EPA by (T235).
        if (HasLostItsDecision(lineStandings, stars))
        {
            return EntrustmentDecisionDueStatus.Revoked;
        }

        if (standing == CommitteeAgendaElsewhereStatus.Missed)
        {
            return EntrustmentDecisionDueStatus.Missed;
        }

        if (partial)
        {
            return EntrustmentDecisionDueStatus.PartialPeriod;
        }

        if (isOpportunistic)
        {
            return EntrustmentDecisionDueStatus.AsOpportunityAllows;
        }

        return window.Semesters[^1] == period
            ? EntrustmentDecisionDueStatus.NotScheduled
            : EntrustmentDecisionDueStatus.DueByYearEnd;
    }

    /// <summary>
    /// Whether a STAR decides <paramref name="window" />, the window of a sitting that issued it: the planner's rule and
    /// the decisions-due page's. (T131 slice 6 review)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Revoked</b> decides nothing: the window must be decided again. <b>Active</b> decides it. So does
    /// <b>Expired</b>: expiry is informational (Decision 10, T139), and a decision that has since lapsed was still taken
    /// in its window, so it neither puts the EPA back on an agenda nor calls a closed window undecided.
    /// </para>
    /// <para>
    /// <b>Superseded</b> decides it when the STAR that superseded it sat outside the window, as a later window's decision
    /// does; or when that one, sitting inside the window, decides it in turn. A second sitting in the same window that
    /// re-decided the EPA replaced the window's decision, so when that one is revoked the window has none: the first STAR
    /// does not stand in again.
    /// </para>
    /// </remarks>
    /// <param name="starsById">
    /// The trainee's STARs from sittings in the window's academic year. A successor absent from it sat for another year,
    /// outside every window of this one.
    /// </param>
    public static bool StarDecides(
        DecisionWindowStar star,
        QuotaWindow window,
        IReadOnlyDictionary<int, DecisionWindowStar> starsById)
    {
        ArgumentNullException.ThrowIfNull(star);
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(starsById);

        var current = star;

        // A chain is as long as the STARs it can pass through; SupersedeBy cannot make a cycle, and this does not trust it.
        for (var hops = 0; hops <= starsById.Count; hops++)
        {
            switch (current.Status)
            {
                case EntrustmentDecisionStatus.Revoked:
                    return false;
                case EntrustmentDecisionStatus.Superseded
                    when current.SupersededById is int successorId &&
                         starsById.TryGetValue(successorId, out var successor) &&
                         window.Covers(successor.Sitting.AcademicYear, successor.Sitting.Semester):
                    current = successor;
                    continue;
                default:
                    return true;
            }
        }

        return true;
    }
}
