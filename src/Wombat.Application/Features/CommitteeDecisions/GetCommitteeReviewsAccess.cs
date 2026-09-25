using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>What the committee reviews page may offer this caller. (T216)</summary>
/// <param name="MaySchedule">
/// Whether the scheduling command and the agenda preview would admit the caller at all
/// (<see cref="CommitteeDecisionAuthorization.MayScheduleReviews" />, the rule both demand first). When false the page offers
/// no Schedule review and opens no form from a link.
/// </param>
/// <param name="TraineeNote">
/// Why the page lists the caller no review and offers them no scheduling, when the reason is the Trainee role they hold
/// beside the role that brought them here (T185's rung); null for anyone else.
/// </param>
public sealed record CommitteeReviewsAccessDto(bool MaySchedule, string? TraineeNote);

/// <summary>
/// What the committee reviews page (<c>/committee/reviews</c>) may offer this caller, read from the rules its commands
/// demand, so the page never decides from the caller's roles (DESIGN.md, T211's rule). (T216)
/// </summary>
public sealed record GetCommitteeReviewsAccessQuery(ClaimsPrincipal Principal) : IRequest<CommitteeReviewsAccessDto>;

public sealed class GetCommitteeReviewsAccessQueryHandler
    : IRequestHandler<GetCommitteeReviewsAccessQuery, CommitteeReviewsAccessDto>
{
    public Task<CommitteeReviewsAccessDto> Handle(GetCommitteeReviewsAccessQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        return Task.FromResult(new CommitteeReviewsAccessDto(
            CommitteeDecisionAuthorization.MayScheduleReviews(request.Principal),
            CommitteeDecisionAuthorization.TraineeNoteOnReviewsPage(request.Principal)));
    }
}
