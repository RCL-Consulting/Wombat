using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Queries;

public sealed record ListDataRightsRequestsQuery(
    DataRightsRequestType? Type,
    DataRightsRequestStatus? Status,
    string? RequesterUserId,
    int Page = 1,
    int PageSize = 25,
    ClaimsPrincipal? Principal = null) : IRequest<PagedDataRightsResult>;

public sealed class ListDataRightsRequestsQueryValidator : AbstractValidator<ListDataRightsRequestsQuery>
{
    public ListDataRightsRequestsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ListDataRightsRequestsQueryHandler : IRequestHandler<ListDataRightsRequestsQuery, PagedDataRightsResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ListDataRightsRequestsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedDataRightsResult> Handle(ListDataRightsRequestsQuery request, CancellationToken cancellationToken)
    {
        if (request.Principal is not null)
            DemandReviewAccess(request.Principal);

        var query = _dbContext.Set<DataRightsRequest>().AsQueryable();

        if (request.Type is not null)
            query = query.Where(r => r.Type == request.Type.Value);

        if (request.Status is not null)
            query = query.Where(r => r.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.RequesterUserId))
            query = query.Where(r => r.RequesterUserId == request.RequesterUserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.RequestedOn)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new DataRightsRequestSummaryDto(
                r.Id,
                r.RequesterDisplayName,
                r.RequestedOn,
                r.Type,
                r.Status))
            .ToListAsync(cancellationToken);

        return new PagedDataRightsResult(items, totalCount, request.Page, request.PageSize);
    }

    private static void DemandReviewAccess(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.Coordinator))
            return;

        throw new UnauthorizedAccessException("Only administrators and coordinators may list all data rights requests.");
    }
}
