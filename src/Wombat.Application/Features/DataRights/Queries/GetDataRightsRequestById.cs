using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Queries;

public sealed record GetDataRightsRequestByIdQuery(
    Guid Id,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class GetDataRightsRequestByIdQueryValidator : AbstractValidator<GetDataRightsRequestByIdQuery>
{
    public GetDataRightsRequestByIdQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetDataRightsRequestByIdQueryHandler : IRequestHandler<GetDataRightsRequestByIdQuery, DataRightsRequestDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDataRightsRequestByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataRightsRequestDto> Handle(GetDataRightsRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");

        DemandAccess(request.Principal, entity);

        return new DataRightsRequestDto(
            entity.Id,
            entity.RequesterUserId,
            entity.RequesterDisplayName,
            entity.RequestedOn,
            entity.Type,
            entity.Status,
            entity.Reason,
            entity.DecisionNote,
            entity.DecidedByUserId,
            entity.DecidedOn,
            entity.CompletedOn);
    }

    private static void DemandAccess(ClaimsPrincipal principal, DataRightsRequest entity)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.Coordinator))
            return;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.Equals(userId, entity.RequesterUserId, StringComparison.Ordinal))
            return;

        throw new UnauthorizedAccessException("You are not authorized to view this request.");
    }
}
