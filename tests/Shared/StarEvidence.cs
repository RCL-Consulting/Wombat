using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Tests.Shared;

/// <summary>
/// The evidence a STAR stored directly by a test rests on (D38, T131). Linked into every test project that stores a
/// STAR without conducting a review.
/// </summary>
/// <remarks>
/// A STAR is issued only with at least one evidence link, and a link is built only from a line of the issuing review's
/// frozen snapshot (<see cref="EntrustmentEvidenceLink.FromSnapshot" />). A fixture that stores a STAR to test something
/// else (a read, a revocation, a certificate) needs one, and the line need not be stored: the link keeps the line's id
/// with no foreign key.
/// </remarks>
public static class StarEvidence
{
    /// <summary>One evidence link, copied from an activity line of a review's snapshot.</summary>
    public static EntrustmentEvidenceLink[] One(int lineId = 1, int activityId = 1)
        =>
        [
            EntrustmentEvidenceLink.FromSnapshot(new CommitteeEvidence
            {
                Id = lineId,
                SourceType = CommitteeEvidenceSourceType.Activity,
                ActivityId = activityId,
                SourceLabel = $"Mini-CEX #{activityId}",
                Summary = "State: completed."
            })
        ];
}
