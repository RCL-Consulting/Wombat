namespace Wombat.Domain.CommitteeDecisions;

public enum CommitteeDecisionCategory
{
    SatisfactoryProgress = 1,
    SatisfactoryWithObservations = 2,
    InadequateProgressAdditionalTraining = 3,
    InadequateProgressRepeat = 4,
    ReleaseFromTraining = 5,
    OutcomeDeferred = 6,

    /// <summary>
    /// Terminal outcome: the trainee has met all requirements and graduates / completes the
    /// programme. Recorded at the final (pre-graduation) review; the trainee profile is then marked
    /// complete via the graduation lifecycle (T080). (T081 / F-5-1)
    /// </summary>
    Graduate = 7
}
