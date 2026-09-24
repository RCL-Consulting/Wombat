using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// The entrustment decisions a panel has staged against one review, before they are issued.
/// </summary>
/// <remarks>
/// Carries the caller because the rows are the committee's working draft about a named trainee —
/// EPA code, the level they are minded to authorise, and the written rationale for it. Naming a
/// review id used to be the whole of the authorization: the page it feeds asked for the review
/// first, so nothing leaked in practice, but a second caller of this query would have found no
/// gate at all. It climbs the same review ladder as GetCommitteeReviewByIdQuery and
/// GetSamplingConcentrationWarningsQuery rather than a rule of its own. (T101 finding E)
/// </remarks>
public sealed record ListPendingEntrustmentDecisionsForReviewQuery(int ReviewId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<PendingEntrustmentDecisionDto>>;

public sealed class ListPendingEntrustmentDecisionsForReviewQueryValidator : AbstractValidator<ListPendingEntrustmentDecisionsForReviewQuery>
{
    public ListPendingEntrustmentDecisionsForReviewQueryValidator()
    {
        RuleFor(query => query.ReviewId).GreaterThan(0);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class ListPendingEntrustmentDecisionsForReviewQueryHandler
    : IRequestHandler<ListPendingEntrustmentDecisionsForReviewQuery, IReadOnlyList<PendingEntrustmentDecisionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListPendingEntrustmentDecisionsForReviewQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PendingEntrustmentDecisionDto>> Handle(ListPendingEntrustmentDecisionsForReviewQuery request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(r => r.Panel)
                .ThenInclude(p => p.Members)
            .SingleOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandReviewAccess(request.Principal, review);

        var pending = await _dbContext.Set<PendingEntrustmentDecision>()
            .AsNoTracking()
            .Include(p => p.Epa)
            .Include(p => p.AuthorisedLevel)
            .Where(p => p.ReviewId == request.ReviewId)
            .OrderBy(p => p.Epa!.Code)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        // T165: which staged decisions ratifying would refuse (T167's rule), so the page can say so and offer Remove on
        // them alone once the decision is recorded and the rest are fixed.
        var refusals = await StarCurriculum.RefusalsForStagedAsync(_dbContext, review.TraineeUserId, pending, cancellationToken);

        return pending
            .Select(p => p.ToDto() with { NoLongerFits = refusals.TryGetValue(p.Id, out var judged) ? judged.Refusal : null })
            .ToArray();
    }
}
