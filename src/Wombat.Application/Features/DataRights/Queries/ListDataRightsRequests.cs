using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.DataRights.Queries;

/// <remarks>
/// <c>Principal</c> sits ahead of the paging parameters deliberately: it is the authorization input,
/// and a security parameter must not be optional. Until T112 it was last AND defaulted to null, and
/// the handler skipped its check when it was — so a caller who passed nothing was authorized by
/// omission.
/// The page size is PagerControls' 20, of 10/20/50/100 (T387, Q18; was a fixed 25). Each row carries the requester's live
/// name and address, or the pseudonym and the erasure's day (F1, S1, N5, N13), and for the Administrator the request's
/// institution (Q23); the result says whose requests they are (S4, S12).
/// </remarks>
public sealed record ListDataRightsRequestsQuery(
    DataRightsRequestType? Type,
    DataRightsRequestStatus? Status,
    string? RequesterUserId,
    ClaimsPrincipal Principal,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedDataRightsResult>;

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
    private readonly IUserAdministrationService _users;

    public ListDataRightsRequestsQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
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

        var rows = await query
            .OrderByDescending(r => r.RequestedOn)
            .ThenByDescending(r => r.Id) // T387 review N6: a tie never swaps rows across a page boundary
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // The page's people and institutions, each read once for the whole page, never per row (T387, F1, Q23).
        var requesterIds = rows.Select(r => r.RequesterUserId).Distinct(StringComparer.Ordinal).ToArray();
        var erasures = await DataRightsPeople.ErasuresOfAsync(_dbContext, requesterIds, cancellationToken);
        var living = requesterIds.Where(id => erasures.Of(id) is null).ToArray();
        var contacts = living.Length == 0
            ? new Dictionary<string, UserContact>(StringComparer.Ordinal)
            : await _users.GetContactsAsync(living, cancellationToken);

        var everyInstitution = request.Principal.IsAdministrator();
        var institutionIds = rows.Select(r => r.InstitutionId).OfType<int>().ToHashSet();
        if (!everyInstitution && request.Principal.GetInstitutionId() is { } ownInstitutionId)
        {
            institutionIds.Add(ownInstitutionId);
        }

        var institutionNames = institutionIds.Count == 0
            ? new Dictionary<int, string>()
            : await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => institutionIds.Contains(institution.Id))
                .ToDictionaryAsync(institution => institution.Id, institution => institution.Name, cancellationToken);

        var items = rows
            .Select(r =>
            {
                var erasure = erasures.Of(r.RequesterUserId);
                var contact = erasure is null ? contacts.GetValueOrDefault(r.RequesterUserId) : null;
                return new DataRightsRequestSummaryDto(
                    r.Id,
                    DataRightsPeople.RequesterName(r, erasure, contact),
                    r.RequestedOn,
                    r.Type,
                    r.Status)
                {
                    RequesterEmail = DataRightsPeople.EmailOf(contact),
                    ErasedOn = erasure is null ? null : ProgrammeCalendar.DateOf(erasure.ErasedOn),
                    // The Institution column is the Administrator's alone (Q23): a scoped reviewer sees one institution's.
                    InstitutionName = everyInstitution && r.InstitutionId is { } institutionId
                        ? institutionNames.GetValueOrDefault(institutionId)
                        : null,
                };
            })
            .ToList();

        return new PagedDataRightsResult(items, totalCount, request.Page, request.PageSize)
        {
            EveryInstitution = everyInstitution,
            // The scoped reviewer's own institution, for the subtitle, the caption and the empty sentence (S4, S12); null
            // for a Coordinator set to no institution, who sees nothing.
            InstitutionName = !everyInstitution && request.Principal.GetInstitutionId() is { } scopedId
                ? institutionNames.GetValueOrDefault(scopedId)
                : null,
        };
    }

}
