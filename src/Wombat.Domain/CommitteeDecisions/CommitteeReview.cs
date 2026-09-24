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

    public int Id { get; set; }
    public string TraineeUserId { get; set; } = string.Empty;
    public int PanelId { get; set; }
    public DateOnly ReviewPeriodFrom { get; set; }
    public DateOnly ReviewPeriodTo { get; set; }
    public DateOnly ScheduledOn { get; set; }
    public bool IsFormative { get; set; }
    public CommitteeReviewType ReviewType { get; set; } = CommitteeReviewType.AnnualProgression;
    public CommitteeReviewState State { get; private set; } = CommitteeReviewState.Scheduled;
    public DateTime? StartedOn { get; private set; }
    public string? StartedByUserId { get; private set; }
    public DateTime? RatifiedOn { get; private set; }
    public string? RatifiedByUserId { get; private set; }
    public DateTime? FinalizedOn { get; private set; }

    public DecisionPanel Panel { get; set; } = null!;
    public ICollection<CommitteeDecision> Decisions { get; private set; } = [];
    public ICollection<CommitteeAppeal> Appeals { get; private set; } = [];
    public ICollection<CommitteeEvidence> EvidenceItems { get; private set; } = [];

    public CommitteeDecision? GetCurrentDecision()
        => Decisions.OrderByDescending(decision => decision.DecidedOn).ThenByDescending(decision => decision.Id).FirstOrDefault();

    public void Start(IEnumerable<CommitteeEvidence> evidenceItems, string actorUserId, DateTime utcNow)
    {
        if (State != CommitteeReviewState.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled reviews can be started.");
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new InvalidOperationException("The starting user is required.");
        }

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
    /// Records the committee's decision, and who was present when it was taken. (T165)
    /// </summary>
    /// <param name="present">
    /// The panel members who sat, as the panel holds them now: at least <see cref="Quorum" /> distinct members, the
    /// recording chair among them in the Chair role. Each is copied onto the decision
    /// (<see cref="CommitteeDecision.Attendees" />) with the role they held, so a later change to the panel does not
    /// rewrite who took it.
    /// </param>
    /// <remarks>
    /// Every check runs before anything is changed. The audit pipeline saves the request's context from its catch, so
    /// a refusal thrown after a mutation would commit it.
    /// </remarks>
    public CommitteeDecision RecordDecision(
        CommitteeDecisionCategory category,
        string rationale,
        string? conditions,
        string actorUserId,
        DateTime utcNow,
        IReadOnlyCollection<DecisionPanelMember> present)
    {
        ArgumentNullException.ThrowIfNull(present);

        if (IsFormative)
        {
            throw new InvalidOperationException("Formative reviews cannot record binding committee decisions. Close the review instead.");
        }

        if (State != CommitteeReviewState.InProgress)
        {
            throw new InvalidOperationException("Only in-progress reviews can record a decision.");
        }

        DemandQuorumPresent(present, actorUserId, recorderIsTheChair: true);

        // Built, and so validated, before the review is touched.
        var decision = CommitteeDecision.Create(category, rationale, conditions, actorUserId, utcNow, present, GetCurrentDecision()?.Id);

        Decisions.Add(decision);
        State = CommitteeReviewState.Decided;
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

    public void Ratify(string actorUserId, DateTime utcNow)
    {
        DemandRatifiable();

        RatifiedByUserId = actorUserId.Trim();
        RatifiedOn = utcNow;
        State = CommitteeReviewState.Ratified;
    }

    /// <summary>
    /// Refuses, without changing anything, unless the review can be ratified now: a summative review, decided, with a
    /// decision whose recorded attendance holds a quorum (T165). The ratify handler runs it before it reads or checks the
    /// staged entrustment decisions and before its first mutation, so a refusal names the real reason and nothing is
    /// issued (T131); <see cref="Ratify" /> runs it again.
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

        if (GetCurrentDecision() is null)
        {
            throw new InvalidOperationException("A review cannot be ratified without a decision.");
        }

        // T165: a decision one person took is not a committee's. Refused before anything changes, so a refused ratify
        // stamps nothing and its handler issues no STAR.
        if (QuorumShortfall() is { } shortfall)
        {
            throw new InvalidOperationException(shortfall);
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
    /// Resolves the open appeal. Upheld and Dismissed leave the committee's decision standing; Remitted replaces it with
    /// a decision the appeal body takes, and like every committee decision that one records who was present. (T165)
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
            if (!remittedCategory.HasValue || string.IsNullOrWhiteSpace(remittedRationale))
            {
                throw new InvalidOperationException("A remitted appeal must record the replacement decision.");
            }

            if (present is null)
            {
                throw new InvalidOperationException(QuorumRule);
            }

            DemandQuorumPresent(present, actorUserId, recorderIsTheChair: false);

            // Built, and so validated, before the appeal or the review is touched.
            replacement = CommitteeDecision.Create(
                remittedCategory.Value,
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
