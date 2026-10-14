using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// One trainee's entrustment decisions that are in force or no longer are (Active, Revoked and Expired), for the trainee
/// themselves or anyone who may read about them (<see cref="TraineeScopeResolver.MayReadAsync" />); empty for anyone
/// else, never a refusal. (T113)
/// </summary>
/// <remarks>
/// A superseded STAR is left off: another STAR on the same EPA replaced it. A revoked one carries its reason, which the
/// trainee reads under her own access control, and its reviser by name (<see cref="EntrustmentDecisionDto.RevokedByName" />),
/// so the page the revocation mail links to says what happened (T367, T319).
/// </remarks>
public sealed record GetDecisionsForTraineeQuery(string TraineeUserId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class GetDecisionsForTraineeQueryValidator : AbstractValidator<GetDecisionsForTraineeQuery>
{
    public GetDecisionsForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetDecisionsForTraineeQueryHandler
    : IRequestHandler<GetDecisionsForTraineeQuery, IReadOnlyList<EntrustmentDecisionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetDecisionsForTraineeQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<IReadOnlyList<EntrustmentDecisionDto>> Handle(GetDecisionsForTraineeQuery request, CancellationToken cancellationToken)
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
            .Where(d => d.TraineeUserId == request.TraineeUserId
                && (d.Status == EntrustmentDecisionStatus.Active
                    || d.Status == EntrustmentDecisionStatus.Revoked
                    || d.Status == EntrustmentDecisionStatus.Expired))
            .OrderBy(d => d.Epa!.Code)
            .ToListAsync(cancellationToken);

        var names = await UserDisplayNames.ResolveAsync(_users, decisions.Select(d => d.RevokedByUserId), cancellationToken);

        return decisions
            .Select(d => d.ToDto() with { RevokedByName = d.RevokedByUserId is null ? null : names.NameOf(d.RevokedByUserId) })
            .ToArray();
    }
}
