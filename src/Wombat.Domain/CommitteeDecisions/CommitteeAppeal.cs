namespace Wombat.Domain.CommitteeDecisions;

public sealed class CommitteeAppeal
{
    private CommitteeAppeal()
    {
    }

    public int Id { get; private set; }
    public int ReviewId { get; private set; }
    public DateTime LodgedOn { get; private set; }
    public string LodgedByUserId { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public DateTime? ResolvedOn { get; private set; }
    public string? ResolvedByUserId { get; private set; }
    public CommitteeAppealOutcome? Outcome { get; private set; }

    public CommitteeReview Review { get; private set; } = null!;

    public static CommitteeAppeal Lodge(string reason, string actorUserId, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("An appeal reason is required.");
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new InvalidOperationException("The appealing user is required.");
        }

        return new CommitteeAppeal
        {
            LodgedOn = utcNow,
            LodgedByUserId = actorUserId.Trim(),
            Reason = reason.Trim()
        };
    }

    public void Resolve(CommitteeAppealOutcome outcome, string actorUserId, DateTime utcNow)
    {
        if (ResolvedOn.HasValue)
        {
            throw new InvalidOperationException("This appeal has already been resolved.");
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new InvalidOperationException("The resolving user is required.");
        }

        Outcome = outcome;
        ResolvedOn = utcNow;
        ResolvedByUserId = actorUserId.Trim();
    }
}
