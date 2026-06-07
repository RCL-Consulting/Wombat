namespace Wombat.Domain.CommitteeDecisions;

public sealed class CommitteeReview
{
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

    public CommitteeDecision RecordDecision(
        CommitteeDecisionCategory category,
        string rationale,
        string? conditions,
        string actorUserId,
        DateTime utcNow)
    {
        if (IsFormative)
        {
            throw new InvalidOperationException("Formative reviews cannot record binding committee decisions. Close the review instead.");
        }

        if (State != CommitteeReviewState.InProgress)
        {
            throw new InvalidOperationException("Only in-progress reviews can record a decision.");
        }

        var decision = CommitteeDecision.Create(category, rationale, conditions, actorUserId, utcNow, GetCurrentDecision()?.Id);
        Decisions.Add(decision);
        State = CommitteeReviewState.Decided;
        return decision;
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

        RatifiedByUserId = actorUserId.Trim();
        RatifiedOn = utcNow;
        State = CommitteeReviewState.Ratified;
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

    public CommitteeAppeal ResolveAppeal(
        CommitteeAppealOutcome outcome,
        string actorUserId,
        DateTime utcNow,
        CommitteeDecisionCategory? remittedCategory = null,
        string? remittedRationale = null,
        string? remittedConditions = null)
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
        if (outcome == CommitteeAppealOutcome.Remitted && (!remittedCategory.HasValue || string.IsNullOrWhiteSpace(remittedRationale)))
        {
            throw new InvalidOperationException("A remitted appeal must record the replacement decision.");
        }

        appeal.Resolve(outcome, actorUserId, utcNow);

        if (outcome == CommitteeAppealOutcome.Remitted)
        {
            Decisions.Add(CommitteeDecision.Create(
                remittedCategory!.Value,
                remittedRationale!,
                remittedConditions,
                actorUserId,
                utcNow,
                GetCurrentDecision()?.Id));
        }

        FinalizedOn = utcNow;
        State = CommitteeReviewState.Final;
        return appeal;
    }
}
