using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

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
            // Same string the gate throws below. RequestDetail.razor renders ex.Message straight into
            // an alert, so a distinct not-found message would tell a Coordinator "this id exists, at
            // another institution" apart from "this id does not exist" — the oracle the single
            // RefusalMessage exists to prevent.
            ?? throw new UnauthorizedAccessException(DataRightsAuthorization.RefusalMessage);

        DataRightsAuthorization.DemandReadAccess(request.Principal, entity);

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

}
