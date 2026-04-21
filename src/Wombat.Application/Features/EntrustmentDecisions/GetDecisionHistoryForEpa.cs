using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record GetDecisionHistoryForEpaQuery(string TraineeUserId, int EpaId) : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class GetDecisionHistoryForEpaQueryValidator : AbstractValidator<GetDecisionHistoryForEpaQuery>
{
    public GetDecisionHistoryForEpaQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.EpaId).GreaterThan(0);
    }
}

public sealed class GetDecisionHistoryForEpaQueryHandler
    : IRequestHandler<GetDecisionHistoryForEpaQuery, IReadOnlyList<EntrustmentDecisionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDecisionHistoryForEpaQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EntrustmentDecisionDto>> Handle(GetDecisionHistoryForEpaQuery request, CancellationToken cancellationToken)
    {
        var decisions = await _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .Where(d => d.TraineeUserId == request.TraineeUserId && d.EpaId == request.EpaId)
            .OrderByDescending(d => d.IssuedOn)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        return decisions.Select(d => d.ToDto()).ToArray();
    }
}
