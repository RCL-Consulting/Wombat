using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Domain.CommitteeDecisions;

public sealed class CommitteeReview
{
    /// <summary>
    /// How many panel members must be recorded as present for a decision to be recorded and ratified: the chair and at
    /// least one other. (T165, D46)
    /// </summary>
    /// <remarks>
    /// The College's summative decision is a Clinical Competency Committee's, "multiple staff members", never one
    /// person's. Two is the floor that word allows. It is one number for every panel until an operator asks for a
    /// per-panel quorum; a panel itself must hold at least this many members (<c>DecisionPanelComposition</c>).
    /// </remarks>
    public const int Quorum = 2;

    /// <summary>The rule a decision's attendance is held to, as the refusals and the review page state it.</summary>
    public const string QuorumRule =
        "A committee decision needs at least two panel members present: the chair and at least one other.";

    /// <summary>
    /// The states in which a review is still open: scheduled, in progress, or decided and not yet ratified. Nothing is
    /// settled at an open review; it is what the panel still has to start, record or ratify, and what
    /// <see cref="Withdraw" /> ends. A review under appeal is not open: its decision is ratified and its STARs issued.
    /// </summary>
    public static readonly IReadOnlyList<CommitteeReviewState> OpenStates =
        [CommitteeReviewState.Scheduled, CommitteeReviewState.InProgress, CommitteeReviewState.Decided];

    /// <summary>The longest <see cref="WithdrawalReason" /> stored.</summary>
    public const int WithdrawalReasonMaxLength = 500;

    /// <summary>The reason an erasure withdraws a trainee's open review with (T258).</summary>
    public const string WithdrawnTraineeErased =
        "Withdrawn because the trainee's personal data was erased at their request. Nothing more is decided at this review.";

    public int Id { get; set; }
    public string TraineeUserId { get; set; } = string.Empty;
    public int PanelId { get; set; }

    /// <summary>
    /// The academic year of the period the review sits for (T131, Decision 4). With <see cref="Semester" /> it is the
    /// <see cref="AcademicPeriod" /> whose decisions the review's agenda holds.
    /// </summary>
    /// <remarks>
    /// Stored, chosen when the review is scheduled, and never derived from <see cref="ReviewPeriodTo" />: the evidence
    /// window is an arbitrary range the scheduler may widen or narrow, and <see cref="AcademicPeriod" /> is not it. The
    /// scheduling page's period select fills the window from the period, and the window stays editable. Required, not
    /// defaulted: the zero value is no year, and a review with no period would plan no agenda.
    /// </remarks>
    public required int AcademicYear { get; set; }

    /// <summary>The semester, 1 or 2, of the period the review sits for (T131, Decision 4).</summary>
    public required int Semester { get; set; }

    public DateOnly ReviewPeriodFrom { get; set; }
    public DateOnly ReviewPeriodTo { get; set; }
    public DateOnly ScheduledOn { get; set; }
    public bool IsFormative { get; set; }
    public CommitteeReviewType ReviewType { get; set; } = CommitteeReviewType.AnnualProgression;

    /// <summary>
    /// Whether the review's decision records a progression category: false for an entrustment-only review, whose decision
    /// is the STARs staged at it (T131 slice 5). Read from the review's own type, fixed when it was scheduled.
    /// </summary>
    public bool DecidesProgression => CommitteeReviewTypes.DecidesProgression(ReviewType);

    public CommitteeReviewState State { get; private set; } = CommitteeReviewState.Scheduled;
    public DateTime? StartedOn { get; private set; }
    public string? StartedByUserId { get; private set; }
    public DateTime? RatifiedOn { get; private set; }
    public string? RatifiedByUserId { get; private set; }
    public DateTime? FinalizedOn { get; private set; }

    /// <summary>When the review was withdrawn (<see cref="Withdraw" />), or null. Set exactly when it is withdrawn (T258).</summary>
    public DateTime? WithdrawnOn { get; private set; }

    /// <summary>
    /// Why the review was withdrawn, as recorded with the withdrawal and shown on the review, or null. Set exactly when it
    /// is withdrawn (T258).
    /// </summary>
    public string? WithdrawalReason { get; private set; }

    public DecisionPanel Panel { get; set; } = null!;
    public ICollection<CommitteeDecision> Decisions { get; private set; } = [];
    public ICollection<CommitteeAppeal> Appeals { get; private set; } = [];
    public ICollection<CommitteeEvidence> EvidenceItems { get; private set; } = [];

    /// <summary>
    /// The EPAs the review is there to decide (T131 slice 4). Planned when a binding review is scheduled and again when it
    /// starts, which only adds; then each line only changes state. A formative review has none. A line whose window another
    /// sitting decides while the review is open reads as decided elsewhere, and is settled so (T235). After Start nothing is
    /// added but a chair's line, by staging.
    /// </summary>
    public ICollection<CommitteeAgendaLine> AgendaLines { get; private set; } = [];

    /// <summary>The academic period the review sits for.</summary>
    public AcademicPeriod Period => new(AcademicYear, Semester);

    public CommitteeDecision? GetCurrentDecision()
        => Decisions.OrderByDescending(decision => decision.DecidedOn).ThenByDescending(decision => decision.Id).FirstOrDefault();

    public void Start(IEnumerable<CommitteeEvidence> evidenceItems, string actorUserId, DateTime utcNow)
        => Start(evidenceItems, [], actorUserId, utcNow);

    /// <summary>
    /// Starts the review: freezes the evidence snapshot, and with it the agenda, after adding the cadence lines planned
    /// now for EPAs it does not yet hold (T131). Nothing is changed unless every check passes.
    /// </summary>
    public void Start(
        IEnumerable<CommitteeEvidence> evidenceItems,
        IEnumerable<CommitteeAgendaLine> agendaLines,
        string actorUserId,
        DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(evidenceItems);
        ArgumentNullException.ThrowIfNull(agendaLines);

        if (State != CommitteeReviewState.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled reviews can be started.");
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new InvalidOperationException("The starting user is required.");
        }

        var lines = agendaLines.ToArray();
        DemandAddableCadenceLines(lines);
        AddNewLines(lines);

        EvidenceItems.Clear();
        foreach (var evidenceItem in evidenceItems)
        {
            EvidenceItems.Add(evidenceItem);
        }

        StartedByUserId = actorUserId.Trim();
        StartedOn = utcNow;
        State = CommitteeReviewState.InProgress;
    }

    /// <summary>
    /// Adds the cadence lines planned for a review not yet started, skipping any EPA it already holds. Planning only adds:
    /// a line already on the agenda keeps its window and state. (T131)
    /// </summary>
    public void AddCadenceLines(IEnumerable<CommitteeAgendaLine> agendaLines)
    {
        ArgumentNullException.ThrowIfNull(agendaLines);

        if (State != CommitteeReviewState.Scheduled)
        {
            throw new InvalidOperationException("An agenda is planned only before the review starts.");
        }

        var lines = agendaLines.ToArray();
        DemandAddableCadenceLines(lines);
        AddNewLines(lines);
    }

    /// <summary>
    /// Adds the line the chair creates by staging a decision on an EPA the agenda does not hold. The routing check is the
    /// staging handler's (<c>AgendaPlanner</c>); this holds the rest. (T131, Decision 3)
    /// </summary>
    /// <remarks>
    /// Only while the review is in progress: staging is (T165), and the agenda is fixed with the decision the panel records
    /// (<see cref="EnsureAgendaSettled" />).
    /// </remarks>
    public CommitteeAgendaLine AddChairLine(CommitteeAgendaLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        if (IsFormative)
        {
            throw new InvalidOperationException(FormativeHasNoAgenda);
        }

        if (State != CommitteeReviewState.InProgress)
        {
            throw new InvalidOperationException("The chair adds to the agenda only while the review is in progress.");
        }

        if (line.Origin != CommitteeAgendaLineOrigin.Chair)
        {
            throw new ArgumentException("Only a chair's line is added while the review sits.", nameof(line));
        }

        if (AgendaLineFor(line.EpaId) is not null)
        {
            throw new InvalidOperationException($"{line.EpaCode} is already on this review's agenda.");
        }

        AgendaLines.Add(line);
        return line;
    }

    /// <summary>
    /// Takes off the agenda the chair's line on this EPA, when the decision that put it there is removed: a chair's line
    /// exists because a decision was staged on it. Returns the line removed, or null when there was none.
    /// </summary>
    public CommitteeAgendaLine? RemoveChairLineFor(int epaId)
    {
        var line = AgendaLineFor(epaId);
        if (line is not { Origin: CommitteeAgendaLineOrigin.Chair, State: CommitteeAgendaLineState.Due })
        {
            return null;
        }

        AgendaLines.Remove(line);
        return line;
    }

    /// <summary>The agenda's line on this EPA, or null.</summary>
    public CommitteeAgendaLine? AgendaLineFor(int epaId) => AgendaLines.FirstOrDefault(line => line.EpaId == epaId);

    /// <summary>
    /// The closing lines still neither staged, deferred nor decided elsewhere, in code order: what keeps the review from
    /// being ratified (Decision 6, T235). <paramref name="stagedEpaIds" /> are the EPAs with a decision staged at the
    /// review.
    /// </summary>
    /// <param name="decidedElsewhereEpaIds">
    /// The EPAs whose line is still due and whose window another sitting has decided (T235), read by the caller by the one
    /// predicate the planner and the decisions-due page share: such a line is optional.
    /// </param>
    public IReadOnlyList<CommitteeAgendaLine> OutstandingClosingLines(
        IEnumerable<int> stagedEpaIds,
        IEnumerable<int> decidedElsewhereEpaIds)
    {
        ArgumentNullException.ThrowIfNull(stagedEpaIds);
        ArgumentNullException.ThrowIfNull(decidedElsewhereEpaIds);

        var staged = stagedEpaIds.ToHashSet();
        var decidedElsewhere = decidedElsewhereEpaIds.ToHashSet();
        return AgendaLines
            .Where(line => line.BlocksRatify(staged.Contains(line.EpaId), decidedElsewhere.Contains(line.EpaId)))
            .OrderBy(line => line.EpaCode, StringComparer.Ordinal)
            .ThenBy(line => line.EpaId)
            .ToArray();
    }

    /// <summary>
    /// Refuses, without changing anything, while a closing line is neither staged, deferred nor decided elsewhere. The
    /// ratify handler runs it before its first mutation; <see cref="CloseAgenda" /> runs it again.
    /// </summary>
    public void EnsureAgendaClosable(IEnumerable<int> stagedEpaIds, IEnumerable<int> decidedElsewhereEpaIds)
    {
        var outstanding = OutstandingClosingLines(stagedEpaIds, decidedElsewhereEpaIds);
        if (outstanding.Count > 0)
        {
            throw new InvalidOperationException(
                "This review cannot be ratified: " +
                OutstandingClosingLinesReason(outstanding.Select(line => line.EpaCode).ToArray()));
        }
    }

    /// <summary>
    /// Refuses, without changing anything, while a closing line is neither staged, deferred nor decided elsewhere: the check
    /// recording the committee's decision runs before its first mutation. (T131 slice 4, T165, T235)
    /// </summary>
    /// <remarks>
    /// The decision the panel records fixes what it decided with it: the staged decisions (T165) and the deferrals, each
    /// the committee's reasoning, so neither changes once it is recorded. A decision recorded with a closing line still
    /// open would leave a review nothing could ratify, so the agenda is settled first, by the predicate ratify enforces.
    /// </remarks>
    public void EnsureAgendaSettled(IEnumerable<int> stagedEpaIds, IEnumerable<int> decidedElsewhereEpaIds)
    {
        var outstanding = OutstandingClosingLines(stagedEpaIds, decidedElsewhereEpaIds);
        if (outstanding.Count > 0)
        {
            throw new InvalidOperationException(
                "The committee's decision cannot be recorded yet: " +
                OutstandingClosingLinesReason(outstanding.Select(line => line.EpaCode).ToArray()));
        }

        // T131 slice 5: an entrustment-only review's decision is what its agenda holds, so one with nothing on it has
        // decided nothing.
        if (EmptyAgendaRefusal() is { } empty)
        {
            throw new InvalidOperationException("The committee's decision cannot be recorded yet: " + empty);
        }
    }

    /// <summary>
    /// The refusal for an entrustment-only review with nothing on its agenda (T131 slice 5): the sentence the record refusal
    /// and the page's disabled Record button share.
    /// </summary>
    public const string NothingOnTheAgenda =
        "Nothing is on this entrustment-only review's agenda, so it has decided nothing. Stage a decision on an EPA this " +
        "panel decides for the trainee.";

    /// <summary>
    /// Why this review's decision cannot be recorded for want of an agenda, or null when it can: an entrustment-only review
    /// decides what its agenda holds, and records no category, so with nothing on its agenda its decision would be a
    /// rationale about nothing. (T131 slice 5)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The agenda is the exact test. Staging a decision always leaves a line (the EPA's cadence line, or the chair's line
    /// staging adds), and deferring needs one; a staged decision removed takes its chair's line with it. A line nothing
    /// was staged on is still a decision when it closes (Deferred, or NotDecided where the College makes it optional).
    /// </para>
    /// <para>
    /// A progression review is never refused by it: its decision is its category. Nor is a formative review, which takes
    /// no decision. Scheduling refuses an entrustment-only review before a panel that decides nothing on the trainee's
    /// curriculum, so the case left is a sitting whose EPAs were all decided earlier in the window, where the chair
    /// stages the one being re-decided.
    /// </para>
    /// <para>
    /// Asked when the decision is recorded, the committee's act, and not again at ratify. A decided review cannot go back
    /// to stage anything, and the one way its agenda empties afterwards is T167's exception, a staged STAR removed because
    /// it could never be issued: refusing ratify then would leave the review decided for good, holding its period's seat.
    /// </para>
    /// </remarks>
    public string? EmptyAgendaRefusal()
        => IsFormative || DecidesProgression || AgendaLines.Count > 0 ? null : NothingOnTheAgenda;

    /// <summary>
    /// Why a review with these closing lines outstanding cannot be ratified, or its decision recorded: the one sentence the
    /// refusals and the page's disabled Ratify and Record buttons share.
    /// </summary>
    public static string OutstandingClosingLinesReason(IReadOnlyList<string> epaCodes)
    {
        ArgumentNullException.ThrowIfNull(epaCodes);

        var codes = epaCodes.Count switch
        {
            0 => string.Empty,
            1 => epaCodes[0],
            _ => string.Join(", ", epaCodes.Take(epaCodes.Count - 1)) + " and " + epaCodes[^1]
        };

        return epaCodes.Count == 1
            ? $"{codes} must be decided at this sitting. Stage a decision on it, or defer it with a reason."
            : $"{codes} must be decided at this sitting. Stage a decision on each, or defer it with a reason.";
    }

    /// <summary>
    /// Closes the agenda as the review is ratified: a staged line is Decided with the STAR issued on it, a line another
    /// sitting has decided in its window is DecidedElsewhere (T235), any other line nothing was staged on is NotDecided, and
    /// a deferred line stays deferred. <paramref name="issuedByEpa" /> are the STARs ratifying issues, by EPA.
    /// </summary>
    /// <param name="decidedElsewhereEpaIds">
    /// The EPAs whose line is still due and whose window another sitting has decided, read by the ratify handler before its
    /// first mutation (<see cref="OutstandingClosingLines" />).
    /// </param>
    public void CloseAgenda(IReadOnlyDictionary<int, EntrustmentDecision> issuedByEpa, IEnumerable<int> decidedElsewhereEpaIds)
    {
        ArgumentNullException.ThrowIfNull(issuedByEpa);
        ArgumentNullException.ThrowIfNull(decidedElsewhereEpaIds);

        var decidedElsewhere = decidedElsewhereEpaIds.ToHashSet();
        EnsureAgendaClosable(issuedByEpa.Keys, decidedElsewhere);

        foreach (var line in AgendaLines.Where(line => line.State == CommitteeAgendaLineState.Due).ToArray())
        {
            if (issuedByEpa.TryGetValue(line.EpaId, out var decision))
            {
                line.Decide(decision);
            }
            else if (decidedElsewhere.Contains(line.EpaId))
            {
                line.SettleDecidedElsewhere();
            }
            else
            {
                line.CloseUndecided();
            }
        }
    }

    /// <summary>The refusal for an agenda on a formative review.</summary>
    public const string FormativeHasNoAgenda = "A formative review carries no agenda: it issues no entrustment decision.";

    /// <summary>The refusal for a progression review's decision with no category (T131 slice 5).</summary>
    public const string ProgressionNeedsACategory =
        "This review decides the trainee's progression, so its decision records a progression category. Choose one.";

    /// <summary>The refusal for an entrustment-only review's decision with a category (T131 slice 5, O4).</summary>
    public const string EntrustmentOnlyRecordsNoCategory =
        "This review decides entrustment only: its decision is the entrustment decisions staged at it, and it records no " +
        "progression category.";

    /// <summary>
    /// Why a decision with <paramref name="category" /> cannot be this review's, or null when it can: a progression review's
    /// decision records one of the categories, an entrustment-only review's none (T131 slice 5). Asked when a decision is
    /// recorded or remitted, before anything changes, and of the current decision when the review is ratified.
    /// </summary>
    public string? CategoryRefusal(CommitteeDecisionCategory? category)
    {
        if (!DecidesProgression)
        {
            return category is null ? null : EntrustmentOnlyRecordsNoCategory;
        }

        return category switch
        {
            null => ProgressionNeedsACategory,
            { } value when !Enum.IsDefined(value) => "Choose a progression category from the list.",
            _ => null
        };
    }

    private void DemandAddableCadenceLines(IReadOnlyCollection<CommitteeAgendaLine> lines)
    {
        if (lines.Count == 0)
        {
            return;
        }

        if (IsFormative)
        {
            throw new InvalidOperationException(FormativeHasNoAgenda);
        }

        if (lines.Any(line => line.Origin != CommitteeAgendaLineOrigin.Cadence))
        {
            throw new ArgumentException("Only cadence lines are planned before the review starts.", nameof(lines));
        }
    }

    private void AddNewLines(IEnumerable<CommitteeAgendaLine> lines)
    {
        foreach (var line in lines)
        {
            if (AgendaLineFor(line.EpaId) is null)
            {
                AgendaLines.Add(line);
            }
        }
    }

    /// <summary>
    /// Records the committee's decision, and who was present when it was taken (T165), and settles the agenda with it: a
    /// line still due, with nothing staged on it, whose window another sitting has decided is written DecidedElsewhere
    /// (T235), so what the decision was taken beside is fixed with it, as the staged decisions and the deferrals are (D46).
    /// </summary>
    /// <param name="present">
    /// The panel members who sat, as the panel holds them now: at least <see cref="Quorum" /> distinct members, the
    /// recording chair among them in the Chair role. Each is copied onto the decision
    /// (<see cref="CommitteeDecision.Attendees" />) with the role they held, so a later change to the panel does not
    /// rewrite who took it.
    /// </param>
    /// <param name="stagedEpaIds">The EPAs with a decision staged at the review.</param>
    /// <param name="decidedElsewhereEpaIds">
    /// The EPAs whose line is still due and whose window another sitting has decided, read by the record handler before
    /// the first mutation, and given to <see cref="EnsureAgendaSettled" /> too. There is no overload without it: every
    /// caller states what it read, so none records a decision that leaves such a line unsettled (T235).
    /// </param>
    /// <remarks>
    /// Every check runs before anything is changed. The audit pipeline saves the request's context from its catch, so
    /// a refusal thrown after a mutation would commit it.
    /// </remarks>
    public CommitteeDecision RecordDecision(
        CommitteeDecisionCategory? category,
        string rationale,
        string? conditions,
        string actorUserId,
        DateTime utcNow,
        IReadOnlyCollection<DecisionPanelMember> present,
        IEnumerable<int> stagedEpaIds,
        IEnumerable<int> decidedElsewhereEpaIds)
    {
        ArgumentNullException.ThrowIfNull(present);
        ArgumentNullException.ThrowIfNull(stagedEpaIds);
        ArgumentNullException.ThrowIfNull(decidedElsewhereEpaIds);

        if (IsFormative)
        {
            throw new InvalidOperationException("Formative reviews cannot record binding committee decisions. Close the review instead.");
        }

        if (State != CommitteeReviewState.InProgress)
        {
            throw new InvalidOperationException("Only in-progress reviews can record a decision.");
        }

        DemandQuorumPresent(present, actorUserId, recorderIsTheChair: true);

        // T131 slice 5: a progression review records a category, an entrustment-only review none.
        if (CategoryRefusal(category) is { } categoryRefusal)
        {
            throw new InvalidOperationException(categoryRefusal);
        }

        // Built, and so validated, before the review is touched.
        var decision = CommitteeDecision.Create(category, rationale, conditions, actorUserId, utcNow, present, GetCurrentDecision()?.Id);

        // T235: the lines another sitting has decided, chosen before anything changes: only a line still due with nothing
        // staged on it, so settling it cannot refuse.
        var staged = stagedEpaIds.ToHashSet();
        var decidedElsewhere = decidedElsewhereEpaIds.ToHashSet();
        var settledElsewhere = AgendaLines
            .Where(line => line.State == CommitteeAgendaLineState.Due &&
                           !staged.Contains(line.EpaId) &&
                           decidedElsewhere.Contains(line.EpaId))
            .ToArray();

        Decisions.Add(decision);
        State = CommitteeReviewState.Decided;

        foreach (var line in settledElsewhere)
        {
            line.SettleDecidedElsewhere();
        }

        return decision;
    }

    /// <summary>
    /// Why the current decision's recorded attendance does not let it be ratified, or null when it does: at least
    /// <see cref="Quorum" /> distinct members were recorded as present for it, one of them in the Chair role. (T165)
    /// </summary>
    public string? QuorumShortfall()
    {
        var attendees = GetCurrentDecision()?.Attendees ?? [];
        var present = attendees.Select(attendee => attendee.UserId).Distinct(StringComparer.Ordinal).Count();
        var chairPresent = attendees.Any(attendee => attendee.Role == DecisionPanelMemberRole.Chair);

        if (present >= Quorum && chairPresent)
        {
            return null;
        }

        if (present == 0)
        {
            return "Nobody was recorded as present when this decision was recorded. " + QuorumRule;
        }

        return chairPresent
            ? "Only the chair was recorded as present when this decision was recorded. " + QuorumRule
            : "The chair was not recorded as present when this decision was recorded. " + QuorumRule;
    }

    /// <summary>
    /// Refuses an attendance that is not a quorum: someone unnamed or named twice, fewer than <see cref="Quorum" />
    /// people, no chair among them, or without the person recording the decision. (T165)
    /// </summary>
    /// <param name="recorderIsTheChair">
    /// At the review the chair records, and must have sat as the chair. At an appeal the appeal body's member records
    /// a remitted decision, the chair or an external member, and must have sat, with the chair among those present.
    /// </param>
    private static void DemandQuorumPresent(IReadOnlyCollection<DecisionPanelMember> present, string actorUserId, bool recorderIsTheChair)
    {
        if (present.Any(member => string.IsNullOrWhiteSpace(member.UserId)))
        {
            throw new InvalidOperationException("Every member recorded as present must be named.");
        }

        var distinct = present.Select(member => member.UserId.Trim()).Distinct(StringComparer.Ordinal).Count();
        if (distinct != present.Count)
        {
            throw new InvalidOperationException("A panel member is recorded as present more than once.");
        }

        if (distinct < Quorum)
        {
            throw new InvalidOperationException(QuorumRule);
        }

        var actor = actorUserId?.Trim();
        bool IsActor(DecisionPanelMember member) => string.Equals(member.UserId.Trim(), actor, StringComparison.Ordinal);

        if (recorderIsTheChair)
        {
            if (!present.Any(member => member.Role == DecisionPanelMemberRole.Chair && IsActor(member)))
            {
                throw new InvalidOperationException("The chair recording the decision must be recorded as present, as the chair.");
            }

            return;
        }

        if (!present.Any(member => member.Role == DecisionPanelMemberRole.Chair))
        {
            throw new InvalidOperationException("The chair must be recorded as present. " + QuorumRule);
        }

        if (!present.Any(IsActor))
        {
            throw new InvalidOperationException("Whoever records the decision must be recorded as present.");
        }
    }

    public void Close(string actorUserId, DateTime utcNow)
    {
        if (!IsFormative)
        {
            throw new InvalidOperationException("Only formative reviews can be closed without a ratified decision. Record a decision and ratify instead.");
        }

        if (State != CommitteeReviewState.InProgress)
        {
            throw new InvalidOperationException("Only in-progress formative reviews can be closed.");
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new InvalidOperationException("The closing user is required.");
        }

        RatifiedByUserId = actorUserId.Trim();
        RatifiedOn = utcNow;
        FinalizedOn = utcNow;
        State = CommitteeReviewState.Final;
    }

    /// <summary>
    /// Withdraws a review that is still open (<see cref="OpenStates" />), recording why: nothing more is started, recorded
    /// or ratified at it (T258). Formative or summative alike.
    /// </summary>
    /// <remarks>
    /// What the review already holds stays as the record of what happened: its evidence snapshot, its agenda, and a decision
    /// recorded but never ratified, which issued nothing. The decisions staged at it are not the review's to keep: its caller
    /// removes them, since only ratifying issues a staged decision and a withdrawn review is never ratified. A review whose
    /// decision is ratified is settled and is refused here. Every check runs before anything changes.
    /// </remarks>
    public void Withdraw(string reason, DateTime utcNow)
    {
        if (!OpenStates.Contains(State))
        {
            throw new InvalidOperationException(
                "Only a review that is scheduled, in progress, or decided and not yet ratified can be withdrawn.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("A withdrawn review records why it was withdrawn.");
        }

        var trimmed = reason.Trim();
        if (trimmed.Length > WithdrawalReasonMaxLength)
        {
            throw new InvalidOperationException(
                $"The reason a review was withdrawn is at most {WithdrawalReasonMaxLength} characters.");
        }

        WithdrawalReason = trimmed;
        WithdrawnOn = utcNow;
        State = CommitteeReviewState.Withdrawn;
    }

    public void Ratify(string actorUserId, DateTime utcNow)
    {
        DemandRatifiable();

        RatifiedByUserId = actorUserId.Trim();
        RatifiedOn = utcNow;
        State = CommitteeReviewState.Ratified;
    }

    /// <summary>
    /// Refuses, without changing anything, unless the review can be ratified now: a summative review, decided, with a
    /// decision whose recorded attendance holds a quorum (T165) and which is the kind the review's type takes (T131 slice
    /// 5): a progression category on a progression review, none on an entrustment-only one. The ratify handler runs it
    /// before it reads or checks the staged entrustment decisions and before its first mutation, so a refusal names the
    /// real reason and nothing is issued (T131); <see cref="Ratify" /> runs it again.
    /// </summary>
    public void DemandRatifiable()
    {
        if (IsFormative)
        {
            throw new InvalidOperationException("Formative reviews are not ratified. Close the review instead.");
        }

        if (State != CommitteeReviewState.Decided)
        {
            throw new InvalidOperationException("Only decided reviews can be ratified.");
        }

        var current = GetCurrentDecision()
            ?? throw new InvalidOperationException("A review cannot be ratified without a decision.");

        // T165: a decision one person took is not a committee's. Refused before anything changes, so a refused ratify
        // stamps nothing and its handler issues no STAR.
        if (QuorumShortfall() is { } shortfall)
        {
            throw new InvalidOperationException(shortfall);
        }

        // T131 slice 5: ratify branches on the type. An entrustment-only review ratifies a decision with no category, a
        // progression review only one with a category. Recording holds both, so this refuses only a decision stored
        // otherwise: a progression review is never ratified with its outcome unrecorded.
        if (CategoryRefusal(current.Category) is not null)
        {
            throw new InvalidOperationException(DecidesProgression
                ? "This review decides the trainee's progression, but its decision records no progression category, so " +
                  "it cannot be ratified."
                : "This review decides entrustment only, but its decision records a progression category, so it cannot " +
                  "be ratified.");
        }
    }

    public CommitteeAppeal LodgeAppeal(string reason, string actorUserId, DateTime utcNow)
    {
        if (State != CommitteeReviewState.Ratified)
        {
            throw new InvalidOperationException("Only ratified reviews can be appealed.");
        }

        if (Appeals.Any(appeal => !appeal.ResolvedOn.HasValue))
        {
            throw new InvalidOperationException("An unresolved appeal already exists for this review.");
        }

        var appeal = CommitteeAppeal.Lodge(reason, actorUserId, utcNow);
        Appeals.Add(appeal);
        State = CommitteeReviewState.UnderAppeal;
        return appeal;
    }

    /// <summary>
    /// Resolves the open appeal, and closes the review for good. Dismissed leaves the committee's decision standing;
    /// Remitted replaces it with a decision the appeal body takes, and like every committee decision that one records who
    /// was present (T165), may record conditions, and is the kind the review's type takes: a progression category on a
    /// progression review, none on an entrustment-only one (T131 slice 5). There is no third outcome (T307, D51): Upheld
    /// did what Dismissed does, under a name that says the appeal succeeded.
    /// </summary>
    /// <param name="present">
    /// Remitted only: the panel members who sat for the replacement decision, held to the quorum the review's own
    /// decision was (<see cref="Quorum" /> distinct members, the chair among them), with the resolver among them.
    /// Before T165 the replacement was one person's: whoever resolved the appeal wrote it alone, and the review went on
    /// showing the original sitting's attendance beside it.
    /// </param>
    public CommitteeAppeal ResolveAppeal(
        CommitteeAppealOutcome outcome,
        string actorUserId,
        DateTime utcNow,
        CommitteeDecisionCategory? remittedCategory = null,
        string? remittedRationale = null,
        string? remittedConditions = null,
        IReadOnlyCollection<DecisionPanelMember>? present = null)
    {
        // T307: refused before anything else is asked or changed, as every precondition below is.
        if (!Enum.IsDefined(outcome))
        {
            throw new InvalidOperationException(CommitteeAppeal.UndefinedOutcome);
        }

        if (State != CommitteeReviewState.UnderAppeal)
        {
            throw new InvalidOperationException("Only reviews under appeal can resolve an appeal.");
        }

        var appeal = Appeals.OrderByDescending(item => item.LodgedOn).ThenByDescending(item => item.Id).FirstOrDefault(item => !item.ResolvedOn.HasValue)
            ?? throw new InvalidOperationException("There is no open appeal to resolve.");

        // Validate every precondition BEFORE mutating any state, so a bad request throws without
        // partially resolving the appeal. (F-4F-1: previously appeal.Resolve() ran first, leaving the
        // appeal marked resolved + the review stranded UnderAppeal when the replacement guard threw.)
        CommitteeDecision? replacement = null;
        if (outcome == CommitteeAppealOutcome.Remitted)
        {
            if (string.IsNullOrWhiteSpace(remittedRationale))
            {
                throw new InvalidOperationException("A remitted appeal must record the replacement decision.");
            }

            // T131 slice 5: the replacement takes a category on a progression review, and none on an entrustment-only one.
            if (CategoryRefusal(remittedCategory) is { } categoryRefusal)
            {
                throw new InvalidOperationException(categoryRefusal);
            }

            if (present is null)
            {
                throw new InvalidOperationException(QuorumRule);
            }

            DemandQuorumPresent(present, actorUserId, recorderIsTheChair: false);

            // Built, and so validated, before the appeal or the review is touched.
            replacement = CommitteeDecision.Create(
                remittedCategory,
                remittedRationale,
                remittedConditions,
                actorUserId,
                utcNow,
                present,
                GetCurrentDecision()?.Id);
        }

        appeal.Resolve(outcome, actorUserId, utcNow);

        if (replacement is not null)
        {
            Decisions.Add(replacement);
        }

        FinalizedOn = utcNow;
        State = CommitteeReviewState.Final;
        return appeal;
    }
}
