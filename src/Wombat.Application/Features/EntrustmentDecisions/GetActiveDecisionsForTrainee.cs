using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record GetActiveDecisionsForTraineeQuery(string TraineeUserId) : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class GetActiveDecisionsForTraineeQueryValidator : AbstractValidator<GetActiveDecisionsForTraineeQuery>
{
    public GetActiveDecisionsForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
    }
}

public sealed class GetActiveDecisionsForTraineeQueryHandler
    : IRequestHandler<GetActiveDecisionsForTraineeQuery, IReadOnlyList<EntrustmentDecisionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetActiveDecisionsForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EntrustmentDecisionDto>> Handle(GetActiveDecisionsForTraineeQuery request, CancellationToken cancellationToken)
    {
        var decisions = await _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .Where(d => d.TraineeUserId == request.TraineeUserId && d.Status == EntrustmentDecisionStatus.Active)
            .OrderBy(d => d.Epa!.Code)
            .ToListAsync(cancellationToken);

        return decisions.Select(d => d.ToDto()).ToArray();
    }
}
