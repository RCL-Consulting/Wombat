using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// The trainee's own committee review, for her review's page at <c>/committee/my-reviews/{id}</c> (T366, flow 09, F8):
/// the review, its decisions, its appeals and its agenda as she reads them, the appeal body named while it is still
/// Ratified, and the panel's institution by name. Null, never a refusal, for anything else.
/// </summary>
/// <param name="Today">The day the agenda's "missed" is judged on; the programme's today when null.</param>
public sealed record GetMyCommitteeReviewQuery(int ReviewId, ClaimsPrincipal Principal, DateOnly? Today = null)
    : IRequest<CommitteeReviewDetailDto?>;

/// <remarks>
/// <para>
/// <b>Only the review's own trainee, and only once it is ratified or later, for every role</b> (F8): another trainee's
/// review, the caller's own before ratification, and an unknown id all read as null, which the page draws as flow 01's not
/// found. An Administrator reads any review at the committee's address (<see cref="GetCommitteeReviewByIdQuery" />), never
/// here: the committee's ladder would show them every review in every state, with a lodge block (round 2, F8).
/// </para>
/// <para>
/// The check is the rule of the ladder's trainee rung, which lodging an appeal also demands
/// (<see cref="CommitteeDecisionAuthorization.DemandOwnRatifiedReview" />), asked whatever role the caller holds; past it
/// the review is described exactly as the committee's page describes it
/// (<see cref="GetCommitteeReviewByIdQueryHandler.DescribeAsync" />), so the two cannot drift.
/// </para>
/// </remarks>
public sealed class GetMyCommitteeReviewQueryHandler : IRequestHandler<GetMyCommitteeReviewQuery, CommitteeReviewDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetMyCommitteeReviewQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeReviewDetailDto?> Handle(GetMyCommitteeReviewQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var review = await GetCommitteeReviewByIdQueryHandler.LoadAsync(_dbContext, request.ReviewId, cancellationToken);
        if (!IsHersToRead(request.Principal, review))
        {
            return null;
        }

        return await GetCommitteeReviewByIdQueryHandler.DescribeAsync(
            _dbContext, _users, review!, request.Principal, request.Today, forItsTrainee: true, cancellationToken);
    }

    /// <summary>The caller's own review, ratified, under appeal or closed: whatever role they act in (F8).</summary>
    private static bool IsHersToRead(ClaimsPrincipal principal, CommitteeReview? review)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return review is not null &&
               !string.IsNullOrEmpty(userId) &&
               string.Equals(review.TraineeUserId, userId, StringComparison.Ordinal) &&
               review.State is CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final;
    }
}
