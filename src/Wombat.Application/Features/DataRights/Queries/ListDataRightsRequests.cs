using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Queries;

/// <remarks>
/// <c>Principal</c> sits ahead of the paging parameters deliberately: it is the authorization input,
/// and a security parameter must not be optional. Until T112 it was last AND defaulted to null, and
/// the handler skipped its check when it was — so a caller who passed nothing was authorized by
/// omission.
/// </remarks>
public sealed record ListDataRightsRequestsQuery(
    DataRightsRequestType? Type,
    DataRightsRequestStatus? Status,
    string? RequesterUserId,
    ClaimsPrincipal Principal,
    int Page = 1,
    int PageSize = 25) : IRequest<PagedDataRightsResult>;

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
        // T112: this used to read `if (request.Principal is not null)`, on a record whose Principal
        // defaulted to null — so a caller who simply passed nothing was authorized by omission. The
        // parameter is now required and the check unconditional.
        if (!DataRightsAuthorization.CanReviewAnything(request.Principal))
        {
            throw new UnauthorizedAccessException(DataRightsAuthorization.RefusalMessage);
        }

        var query = _dbContext.Set<DataRightsRequest>().AsQueryable();

        // ...and the role check alone is not the boundary: it says the caller reviews SOMETHING, not
        // whose. A global Administrator sees every institution; everyone else sees their own, and a
        // reviewer with no institution claim sees nothing rather than everything.
        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            query = scopedInstitutionId.HasValue
                ? query.Where(r => r.InstitutionId == scopedInstitutionId.Value)
                : query.Where(_ => false);
        }

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

}
