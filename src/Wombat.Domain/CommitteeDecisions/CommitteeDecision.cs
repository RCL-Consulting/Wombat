namespace Wombat.Domain.CommitteeDecisions;

public sealed class CommitteeDecision
{
    private CommitteeDecision()
    {
    }

    public int Id { get; private set; }
    public int ReviewId { get; private set; }

    /// <summary>
    /// The progression outcome, or null on the decision of an entrustment-only review, whose decision is the STARs staged
    /// at it (T131 slice 5). Which a decision must carry is its review's type (<see cref="CommitteeReviewTypes" />), held
    /// by <see cref="CommitteeReview" /> when the decision is recorded, remitted or ratified.
    /// </summary>
    public CommitteeDecisionCategory? Category { get; private set; }
    public string Rationale { get; private set; } = string.Empty;
    public string? Conditions { get; private set; }
    public DateTime DecidedOn { get; private set; }
    public string DecidedByChairUserId { get; private set; } = string.Empty;
    public int? SupersedesDecisionId { get; private set; }

    public CommitteeReview Review { get; private set; } = null!;

    /// <summary>
    /// The panel members recorded as present when this decision was taken, with the role each held then (T165). Written
    /// once, when the decision is created, and never changed.
    /// </summary>
    public ICollection<CommitteeDecisionAttendee> Attendees { get; private set; } = [];

    /// <summary>
    /// A decision and who was present for it. Callers go through <see cref="CommitteeReview" />, which holds the
    /// attendance to the quorum before it calls this; this only refuses what no decision can be.
    /// </summary>
    public static CommitteeDecision Create(
        CommitteeDecisionCategory? category,
        string rationale,
        string? conditions,
        string chairUserId,
        DateTime utcNow,
        IReadOnlyCollection<DecisionPanelMember> present,
        int? supersedesDecisionId = null)
    {
        ArgumentNullException.ThrowIfNull(present);

        if (string.IsNullOrWhiteSpace(rationale))
        {
            throw new InvalidOperationException("A committee decision rationale is required.");
        }

        if (string.IsNullOrWhiteSpace(chairUserId))
        {
            throw new InvalidOperationException("The deciding chair user is required.");
        }

        var decision = new CommitteeDecision
        {
            Category = category,
            Rationale = rationale.Trim(),
            Conditions = string.IsNullOrWhiteSpace(conditions) ? null : conditions.Trim(),
            DecidedOn = utcNow,
            DecidedByChairUserId = chairUserId.Trim(),
            SupersedesDecisionId = supersedesDecisionId
        };

        foreach (var member in present)
        {
            decision.Attendees.Add(new CommitteeDecisionAttendee { UserId = member.UserId.Trim(), Role = member.Role });
        }

        return decision;
    }

    public void Amend(CommitteeDecisionCategory? category, string rationale, string? conditions)
        => throw new InvalidOperationException("Committee decisions are immutable. Record a new decision through the appeal workflow.");
}
