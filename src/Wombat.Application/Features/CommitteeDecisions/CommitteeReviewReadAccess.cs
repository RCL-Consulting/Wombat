using System.Security.Claims;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// The committee review read ladder (<see cref="CommitteeDecisionAuthorization.ReadableReviewsAsync" />) for a caller
/// outside this assembly: the weekly coordinator digest, which lists each recipient only the scheduled reviews they could
/// open (T117). A door onto the one rule, not a second rule; the ladder and the demands that refuse by it stay internal.
/// </summary>
/// <remarks>
/// The ladder's set form, not a per-review test: since T218 a speciality or sub-speciality administrator reads a review
/// by where its trainee trains, which the ladder resolves itself, once for every review, and only for a caller whose rung
/// asks it. The ladder is held to the review queries by <c>CommitteeReviewReadLadderTests</c>; the digest's use of this
/// door by <c>WeeklyCoordinatorDigestJobTests</c>.
/// </remarks>
public static class CommitteeReviewReadAccess
{
    /// <summary>
    /// The reviews among <paramref name="reviews" /> this principal may read, as the review page decides. Each review's
    /// <see cref="CommitteeReview.Panel" /> and its members must be loaded.
    /// </summary>
    public static Task<IReadOnlyList<CommitteeReview>> ReadableAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        IReadOnlyCollection<CommitteeReview> reviews,
        CancellationToken cancellationToken)
        => CommitteeDecisionAuthorization.ReadableReviewsAsync(dbContext, principal, reviews, cancellationToken);
}
