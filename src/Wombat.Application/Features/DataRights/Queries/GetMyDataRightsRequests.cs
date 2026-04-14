using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Queries;

public sealed record GetMyDataRightsRequestsQuery(
    ClaimsPrincipal Principal) : IRequest<IReadOnlyList<DataRightsRequestSummaryDto>>;

public sealed class GetMyDataRightsRequestsQueryValidator : AbstractValidator<GetMyDataRightsRequestsQuery>
{
    public GetMyDataRightsRequestsQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetMyDataRightsRequestsQueryHandler : IRequestHandler<GetMyDataRightsRequestsQuery, IReadOnlyList<DataRightsRequestSummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyDataRightsRequestsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DataRightsRequestSummaryDto>> Handle(GetMyDataRightsRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        return await _dbContext.Set<DataRightsRequest>()
            .Where(r => r.RequesterUserId == userId)
            .OrderByDescending(r => r.RequestedOn)
            .Select(r => new DataRightsRequestSummaryDto(
                r.Id,
                r.RequesterDisplayName,
                r.RequestedOn,
                r.Type,
                r.Status))
            .ToListAsync(cancellationToken);
    }
}
