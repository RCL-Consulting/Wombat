using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// Every entrustment decision on one EPA for one trainee, newest first, for the trainee themselves or anyone who may
/// read about them (<see cref="TraineeScopeResolver.MayReadAsync" />); empty for anyone else, never a refusal. (T113)
/// </summary>
public sealed record GetDecisionHistoryForEpaQuery(string TraineeUserId, int EpaId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class GetDecisionHistoryForEpaQueryValidator : AbstractValidator<GetDecisionHistoryForEpaQuery>
{
    public GetDecisionHistoryForEpaQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.EpaId).GreaterThan(0);
        RuleFor(query => query.Principal).NotNull();
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
        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, request.TraineeUserId, cancellationToken))
        {
            return [];
        }

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
