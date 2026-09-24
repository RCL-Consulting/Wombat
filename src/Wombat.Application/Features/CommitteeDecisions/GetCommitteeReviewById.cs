using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record GetCommitteeReviewByIdQuery(int ReviewId, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class GetCommitteeReviewByIdQueryHandler : IRequestHandler<GetCommitteeReviewByIdQuery, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetCommitteeReviewByIdQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeReviewDetailDto> Handle(GetCommitteeReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        // The ladder this handler used to spell out inline now lives beside the other committee
        // guards, because the two sibling queries on the same page have to climb the identical one.
        // The panel carries its own institution regardless of scope; the discipline is national
        // now (T091), so no further lookup is needed to place a review. (T101 finding E)
        CommitteeDecisionAuthorization.DemandReviewAccess(request.Principal, review);

        // T142. The trainee by name, looked up only once the caller has passed the review ladder above. T165 adds the
        // panel's members and those recorded as present at each decision, in the same one lookup.
        var detail = review.ToDetailDto();
        var names = await UserDisplayNames.ResolveAsync(
            _users,
            detail.PanelMembers
                .Concat(detail.Decisions.SelectMany(decision => decision.Attendees))
                .Select(person => person.UserId)
                .Prepend(review.TraineeUserId),
            cancellationToken);

        var seated = await SeatedAsync(review, request.Principal, cancellationToken);

        return detail with
        {
            TraineeName = names.NameOf(review.TraineeUserId),
            PanelMembers = detail.PanelMembers
                .Select(person => person with
                {
                    Name = names.NameOf(person.UserId),
                    MaySit = seated.Contains(person.UserId)
                })
                .ToArray(),
            Decisions = detail.Decisions
                .Select(decision => decision with
                {
                    Attendees = decision.Attendees.Select(person => person with { Name = names.NameOf(person.UserId) }).ToArray()
                })
                .ToArray()
        };
    }

    /// <summary>
    /// The panel members who may be recorded as present at this review now (<see cref="PanelSeat" />, T165): the ones
    /// the record-decision and remit forms offer, by the rule recording enforces. Asked only while the review can still
    /// take a decision, a summative review not yet decided or under appeal, and never for the trainee whose review it
    /// is: who may sit on their panel is the panel's business.
    /// </summary>
    private async Task<IReadOnlySet<string>> SeatedAsync(
        CommitteeReview review,
        System.Security.Claims.ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var mayStillDecide = !review.IsFormative &&
                             review.State is CommitteeReviewState.Scheduled
                                 or CommitteeReviewState.InProgress
                                 or CommitteeReviewState.UnderAppeal;
        var askedByTheTrainee = string.Equals(
            principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            review.TraineeUserId,
            StringComparison.Ordinal);

        if (!mayStillDecide || askedByTheTrainee)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var eligible = await PanelSeat.EligibleAsync(_users, review.Panel.InstitutionId, cancellationToken);
        return review.Panel.Members
            .Select(member => member.UserId)
            .Where(userId => eligible.ContainsKey(userId) &&
                             !string.Equals(userId, review.TraineeUserId, StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
    }
}
