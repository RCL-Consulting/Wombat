namespace Wombat.Domain.CommitteeDecisions;

public enum CommitteeReviewState
{
    Scheduled = 1,
    InProgress = 2,
    Decided = 3,
    Ratified = 4,
    UnderAppeal = 5,
    Final = 6
}
