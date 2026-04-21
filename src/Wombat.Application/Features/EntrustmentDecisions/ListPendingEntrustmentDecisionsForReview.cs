using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record ListPendingEntrustmentDecisionsForReviewQuery(int ReviewId)
    : IRequest<IReadOnlyList<PendingEntrustmentDecisionDto>>;

public sealed class ListPendingEntrustmentDecisionsForReviewQueryValidator : AbstractValidator<ListPendingEntrustmentDecisionsForReviewQuery>
{
    public ListPendingEntrustmentDecisionsForReviewQueryValidator()
    {
        RuleFor(query => query.ReviewId).GreaterThan(0);
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
        var pending = await _dbContext.Set<PendingEntrustmentDecision>()
            .AsNoTracking()
            .Include(p => p.Epa)
            .Include(p => p.AuthorisedLevel)
            .Where(p => p.ReviewId == request.ReviewId)
            .OrderBy(p => p.Epa!.Code)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        return pending.Select(p => p.ToDto()).ToArray();
    }
}
