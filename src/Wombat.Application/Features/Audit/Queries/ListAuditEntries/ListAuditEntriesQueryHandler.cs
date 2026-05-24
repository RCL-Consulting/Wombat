using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit.Queries.ListAuditEntries;

public sealed class ListAuditEntriesQueryHandler : IRequestHandler<ListAuditEntriesQuery, PagedAuditResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ListAuditEntriesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedAuditResult> Handle(ListAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var from = request.From ?? DateTime.UtcNow.AddHours(-24);
        var to = request.To ?? DateTime.UtcNow;

        var query = _dbContext.Set<AuditEntry>()
            .Where(e => e.OccurredAt >= from && e.OccurredAt <= to);

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return new PagedAuditResult(Array.Empty<AuditEntryDto>(), 0, request.Page, request.PageSize);
            }
            // InstitutionalAdmin sees their institution's entries plus global (no-institution) entries.
            var institutionId = scopedInstitutionId.Value;
            query = query.Where(e => e.InstitutionId == null || e.InstitutionId == institutionId);
        }

        if (!string.IsNullOrWhiteSpace(request.ActorUserId))
            query = query.Where(e => e.ActorUserId == request.ActorUserId);

        if (request.Category is not null)
            query = query.Where(e => e.Category == request.Category.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(e => e.Action.Contains(request.Action));

        if (!string.IsNullOrWhiteSpace(request.SubjectType))
            query = query.Where(e => e.SubjectType == request.SubjectType);

        if (request.SuccessOnly is true)
            query = query.Where(e => e.Success);
        else if (request.SuccessOnly is false)
            query = query.Where(e => !e.Success);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new AuditEntryDto(
                e.Id,
                e.OccurredAt,
                e.ActorUserId,
                e.ActorDisplay,
                e.ActorIpAddress,
                e.Category,
                e.Action,
                e.SubjectType,
                e.SubjectId,
                e.InstitutionId,
                e.SpecialityId,
                e.SummaryJson,
                e.Success,
                e.ErrorMessage))
            .ToListAsync(cancellationToken);

        return new PagedAuditResult(items, totalCount, request.Page, request.PageSize);
    }
}
