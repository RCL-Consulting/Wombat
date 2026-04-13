namespace Wombat.Domain.CommitteeDecisions;

public sealed class CommitteeDecision
{
    private CommitteeDecision()
    {
    }

    public int Id { get; private set; }
    public int ReviewId { get; private set; }
    public CommitteeDecisionCategory Category { get; private set; }
    public string Rationale { get; private set; } = string.Empty;
    public string? Conditions { get; private set; }
    public DateTime DecidedOn { get; private set; }
    public string DecidedByChairUserId { get; private set; } = string.Empty;
    public int? SupersedesDecisionId { get; private set; }

    public CommitteeReview Review { get; private set; } = null!;

    public static CommitteeDecision Create(
        CommitteeDecisionCategory category,
        string rationale,
        string? conditions,
        string chairUserId,
        DateTime utcNow,
        int? supersedesDecisionId = null)
    {
        if (string.IsNullOrWhiteSpace(rationale))
        {
            throw new InvalidOperationException("A committee decision rationale is required.");
        }

        if (string.IsNullOrWhiteSpace(chairUserId))
        {
            throw new InvalidOperationException("The deciding chair user is required.");
        }

        return new CommitteeDecision
        {
            Category = category,
            Rationale = rationale.Trim(),
            Conditions = string.IsNullOrWhiteSpace(conditions) ? null : conditions.Trim(),
            DecidedOn = utcNow,
            DecidedByChairUserId = chairUserId.Trim(),
            SupersedesDecisionId = supersedesDecisionId
        };
    }

    public void Amend(CommitteeDecisionCategory category, string rationale, string? conditions)
        => throw new InvalidOperationException("Committee decisions are immutable. Record a new decision through the appeal workflow.");
}
