namespace Wombat.Domain.CommitteeDecisions;

public enum CommitteeReviewState
{
    Scheduled = 1,
    InProgress = 2,
    Decided = 3,
    Ratified = 4,
    UnderAppeal = 5,
    Final = 6,

    /// <summary>
    /// Ended while it was still open, before anything was ratified, with its reason recorded
    /// (<see cref="CommitteeReview.Withdraw" />). Nothing more is decided at it. Only erasure withdraws a review today: the
    /// trainee is gone, so the panel is not left a review of a pseudonym to start, record or ratify (T258).
    /// </summary>
    Withdrawn = 7
}
