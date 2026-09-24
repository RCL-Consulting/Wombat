using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// One trainee's active entrustment decisions, for the trainee themselves or anyone who may read about them
/// (<see cref="TraineeScopeResolver.MayReadAsync" />); empty for anyone else, never a refusal. (T113)
/// </summary>
public sealed record GetActiveDecisionsForTraineeQuery(string TraineeUserId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class GetActiveDecisionsForTraineeQueryValidator : AbstractValidator<GetActiveDecisionsForTraineeQuery>
{
    public GetActiveDecisionsForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
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
        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, request.TraineeUserId, cancellationToken))
        {
            return [];
        }

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
