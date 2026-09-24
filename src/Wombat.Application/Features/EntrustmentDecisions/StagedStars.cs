namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// The STARs staged at a committee review are part of the decision its panel records. (T165, D46)
/// </summary>
/// <remarks>
/// A review's quorum is recorded when its decision is, and ratifying then issues every staged STAR as the committee's.
/// So the staged set is fixed at that moment: staged, changed or removed only while the review is in progress. Before
/// T165 it stayed open on a decided review, and the chair alone, after the sitting, could add, raise or drop a STAR that
/// ratifying then issued as the committee's decision. The one exception is removing a staged STAR that no longer fits
/// the trainee's curriculum, which can never be issued (<see cref="StarCurriculum.RefusalsForStagedAsync" />).
/// </remarks>
public static class StagedStars
{
    public const string FixedWhenDecided =
        "Entrustment decisions are staged with the committee's decision and fixed when it is recorded: they can be " +
        "staged, changed or removed only while the review is in progress.";
}
